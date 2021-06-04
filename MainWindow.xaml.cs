using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BrowseMan browseMan;
        public MainWindow()
        {
            //System.IO.Directory.GetFiles("Downloads");
            InitializeComponent();
            Console.SetOut(new ControlWriter(cons));
            FileGridMan gridMan = new FileGridMan();
            gridMan.fileslist = fileslist;
            gridMan.loadFiles();
            browseMan = new BrowseMan();
            browseMan.search_result_view = browselist;
            browseMan.loadSearched("");
        }

        private void searchbutton_Click(object sender, RoutedEventArgs e)
        {
            browseMan.search_result_view.Items.Clear();
            browseMan.searchedFiles.Clear();
            browseMan.loadSearched(searchbar.Text);
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                searchbutton_Click(sender, e);
            }
        }
    }

    public class WindowMan
    {
        string console_text;
        public WindowMan()
        {
            console_text = "";
        }
        public void Write(string text)
        {
            console_text += text;
        }
    }
    public class ControlWriter : TextWriter
    {
        private TextBlock textbox;
        public ControlWriter(TextBlock textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Text += value;
        }

        public override void Write(string value)
        {
            textbox.Text += value;
        }


        public override void WriteLine(string value)
        {
            textbox.Text += value + '\n';
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }

    }
    public class BrowseMan
    {


        
        public ListView search_result_view { set; get; }
        public List<Tuple<string, string, string>> searchedFiles = new List<Tuple<string, string, string>>();
        HttpClient client = new HttpClient();
        const string books_url= "https://localhost:44351/api/Books/";
        string search_url;
        public async void loadSearched(string search)
        {

            if (search.Length == 1 && search[0] >= '1' && search[0] <= '9')
                search_url =books_url+ search;
            else
            {
                    search_url = books_url;
                
            }

            HttpResponseMessage response = await client.GetAsync(search_url);
            if (response.IsSuccessStatusCode)
            {
                Tuple<string, string, string> entry;
                string content = await response.Content.ReadAsStringAsync();
                try { var parsedObject = JArray.Parse(content);

                    
                    foreach (var thing in parsedObject)
                    {
                        var parsed = JObject.Parse(thing.ToString());
                        entry = new Tuple<string, string, string>(parsed["Id"].ToString(), parsed["Title"].ToString(), parsed["Author"].ToString());
                        if(search.Length == 0 || search.Length == 1 || (search.Length>1 && (entry.Item1.ToLower().Contains(search.ToLower()) || entry.Item2.ToLower().Contains(search.ToLower())|| entry.Item3.ToLower().Contains(search.ToLower()))))
                            search_result_view.Items.Add(entry.Item1 + ". \"" + entry.Item2 + "\" by " + entry.Item3);
                      
                    }
                }
                catch(Newtonsoft.Json.JsonReaderException e)
                {
                    var parsedObject = JObject.Parse(content.ToString());
                    entry = new Tuple<string, string, string>(parsedObject["Id"].ToString(), parsedObject["Title"].ToString(), parsedObject["Author"].ToString());
                    if (search.Length == 0 ||search.Length==1 || (search.Length > 1 && (entry.Item1.ToLower().Contains(search.ToLower()) || entry.Item2.ToLower().Contains(search.ToLower()) || entry.Item3.ToLower().Contains(search.ToLower()))))
                        search_result_view.Items.Add(entry.Item1 + ". \"" + entry.Item2 + "\" by " + entry.Item3);
                   
                }
                

            }
            else
            {
                MessageBox.Show("ERROR:Http response "+response.StatusCode +"\nREASON: "+response.ReasonPhrase);
            }
        }


    }
    public class FileGridMan
    {
        public DirectoryInfo project_directory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
        public DirectoryInfo icons_directory = null;
        public DirectoryInfo downloads_directory = null;
        public List<ListFileItem> loadedFiles;
        public List<FileInfo> fileInfos;
        BitmapImage cbz_ico;
        BitmapImage mobi_ico;
        BitmapImage pdf_ico;
        BitmapImage epub_ico;
        //BitmapImage epub2_ico;
        BitmapImage chm_ico;
        public ListView fileslist { get; set; }

        #region DownloadsGrid
        public FileGridMan()
        {


            foreach (var i in project_directory.GetDirectories())
            {
                if (i.Name == "FileIcons")
                    icons_directory = i;

            }
            if (icons_directory != null)
            {
                cbz_ico = new BitmapImage(new Uri(icons_directory.FullName + "\\cbz_icon.png"));
                chm_ico = new BitmapImage(new Uri(icons_directory.FullName + "\\chm_icon.png"));
                epub_ico = new BitmapImage(new Uri(icons_directory.FullName + "\\epub_icon.png"));
                mobi_ico = new BitmapImage(new Uri(icons_directory.FullName + "\\mobi_icon.png"));
                pdf_ico = new BitmapImage(new Uri(icons_directory.FullName + "\\pdf_icon.png"));
            }
            else
            {
                MessageBox.Show("Couldn't find icon folder!");
            }


        }

        public void loadFiles()
        {
            loadedFiles = new List<ListFileItem>(50);

            Dictionary<string, BitmapImage> image_dict = new Dictionary<string, BitmapImage>();


            Console.WriteLine("Current working directory is {0}", Directory.GetCurrentDirectory());
            Console.WriteLine("Project directory is {0}", project_directory.FullName);
            //Console.WriteLine("Files in this folder are:",  Directory.GetFiles( Directory.GetCurrentDirectory()));
            Console.WriteLine("Folders in project directory: ");
            foreach (var i in project_directory.GetDirectories())
            {
                Console.WriteLine(i.Name);
                if (i.Name == "Downloads")
                    downloads_directory = i;
                if (i.Name == "FileIcons")
                    icons_directory = i;
            }



            foreach (var i in icons_directory.GetFiles())
            {

                image_dict.Add(System.IO.Path.GetFileNameWithoutExtension(i.Name), new BitmapImage(new System.Uri(i.FullName)));
            }

            Console.WriteLine("Files in the download directory are: ");
            fileInfos = new List<FileInfo>(downloads_directory.GetFiles());
            ListFileItem item;
            BitmapImage replacement_image = new BitmapImage();
            foreach (var i in fileInfos)
            {
                Console.WriteLine(i.Name + " with extention " + i.Extension);
                switch (i.Extension)
                {
                    case ".cbz":
                        {
                            item = new ListFileItem(i, ref cbz_ico);
                            break;
                        }
                    case ".pdf":
                        {
                            item = new ListFileItem(i, ref pdf_ico);
                            break;
                        }
                    case ".mobi":
                        {
                            item = new ListFileItem(i, ref mobi_ico);
                            break;
                        }
                    case ".epub":
                        {
                            item = new ListFileItem(i, ref epub_ico);
                            break;
                        }
                    case ".chm":
                        {
                            item = new ListFileItem(i, ref chm_ico);
                            break;
                        }
                    default:
                        {
                            item = new ListFileItem(i, ref replacement_image);
                            break;
                        }
                }

                loadedFiles.Add(item);
                fileslist.Items.Add(item);

            }
        }
        #endregion

    }
    public class ListFileItem
    {
        public FileInfo fileInfo { set; get; }
        public BitmapImage image { set; get; }
        public ListFileItem(FileInfo f, ref BitmapImage i)
        {
            fileInfo = f; image = i;
        }
    }

    
}
