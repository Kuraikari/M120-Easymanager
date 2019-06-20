using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Windows.Markup;
using System.Globalization;

namespace M120_EasyEZ
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent(
        "Tap", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Button));

        public ConnectionBuilder ConnectionBuilder { get; set; }
        public int userId;
        public int TextBoxID = 0;

        public DialogWindow dialogWindow;

        private Window darkwindow;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            this.WindowState = WindowState.Maximized;
        }

        private void InitializeDatabase()
        {
            this.ConnectionBuilder = new ConnectionBuilder();
            this.ConnectionBuilder.SetDatabase("usermanager")
                                              .SetPassword("gibbiX12345")
                                              .SetPort(3306)
                                              .SetServer("127.0.0.1")
                                              .SetUserID("root")
                                              .BuildConnectionString()
                                              .Connect();
        }

        private double GetWindowSizeRatio(Size previousSize, Size newSize)
        {
            return (previousSize.Height + newSize.Height) / (previousSize.Width + newSize.Width);
        }

        public event RoutedEventHandler Tap
        {
            add { AddHandler(TapEvent, value); }
            remove { RemoveHandler(TapEvent, value); }
        }

        private void Ok()
        {
            DynamicObjectFactory objectFactory = new DynamicObjectFactory();
            objectFactory.New(true, true, new string[] { "Age", "Test2", "Testing", "IfThisWorksIAmAGod" });
            objectFactory.New(true, true, new string[] { "NoAge", "MyTest", "IfTestingSucceeds", "WhyIsMyLifeSoDepressing" });
            List<dynamic> objs = objectFactory.GetDynamicList();
            foreach(dynamic o in objs) {
                IDictionary<string, object> propertyValues = (IDictionary<string, object>)o;
                propertyValues.Keys.ToList().ForEach(x => Console.WriteLine(x));
            }
            objectFactory = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Check the text lengths of various textboxes. 
            if (CheckTextLength(txtEmail, 6) && CheckTextLength(txtPassword, 8) && CheckTextLength(txtUser, 6) && CheckTextLength(txtProfile, 6))
            {
                //Make A select statement (SQL) to get the user with the corresponding Email-address 
                this.ConnectionBuilder.SelectEntry("user", new string[] { "id_user", "password" }, true, new string[] { "email" }, new string[] { txtEmail.Text });

                if (this.ConnectionBuilder.Result.col.Count > 1)
                {
                    Console.WriteLine("YOOOOOOOOOOOOOOOO");

                    //Check if Password is correct
                    if (GetHash(txtPassword.Password) == this.ConnectionBuilder.Result.col.Find(x => x.Equals(GetHash(txtPassword.Password))))
                    {
                        Console.WriteLine(this.ConnectionBuilder.Result.col.Find(x => x.Equals(GetHash(txtPassword.Password))));
                        //Get all data from the user table
                        this.ConnectionBuilder.SelectEntry("user", new string[] { "*" }, true, new string[] { "email" }, new string[] { txtEmail.Text });
                    }
                    else
                    {
                        //Paint background red and foreground white if password is incorrect
                        ColorControls(txtPassword, GetSolidColorBrush(Colors.Red), GetSolidColorBrush(Colors.White));
                        return;
                    }
                }
                else
                {
                    Dictionary<string, object> tmp = new Dictionary<string, object>
                    {
                        { "username", txtUser.Text },
                        { "email", txtEmail.Text },
                        { "role_id", comboBox.SelectedIndex },
                        { "password", GetHash(txtPassword.Password) }
                    };

                    this.ConnectionBuilder.CreateEntry("user", tmp);

                    Dictionary<string, object> tmp2 = new Dictionary<string, object>
                    {
                        { "profilename", txtProfile.Text },
                        { "user_id", ConnectionBuilder.LastEntry.Last().Value },
                    };

                    this.ConnectionBuilder.CreateEntry("profile", tmp2);
                }
                gridRegister.Visibility = Visibility.Collapsed;
                grdProfilename.Visibility = Visibility.Visible;

                this.ConnectionBuilder.SelectEntry("profile", new string[] { "*" }, true, new string[] { "user_id" }, new string[] { ConnectionBuilder.Result.col[0] });
                int.TryParse(ConnectionBuilder.Result.col[2], out userId);

                List<Profile> profiles = new List<Profile>();
                for (int i = 0; i < ConnectionBuilder.Result.col.Count; i += 4)
                {
                    if (ConnectionBuilder.Result.col[i] == "*")
                    {
                        i = 1;
                    }

                    if (!(ConnectionBuilder.Result.col[i] == "*"))
                    {
                        profiles.Add(new Profile(int.Parse(ConnectionBuilder.Result.col[i]), ConnectionBuilder.Result.col[i + 1], int.Parse(ConnectionBuilder.Result.col[i + 2])));
                    }
                }
                dataGrid.ItemsSource = profiles;

                cmdRegister.Visibility = Visibility.Collapsed;
                cmdCreateEntry.Visibility = Visibility.Visible;
            }
            else
            {
                CheckForm();
            }
        }

        private void CheckForm()
        {
            SolidColorBrush redBrush = GetSolidColorBrush(Colors.Red);
            SolidColorBrush whiteBrush = GetSolidColorBrush(Colors.White);
            if (!CheckTextLength(txtEmail, 6))
            {
                ColorControls(txtEmail, redBrush, whiteBrush);
            }
            if (!CheckTextLength(txtPassword, 8))
            {
                ColorControls(txtPassword, redBrush, whiteBrush);
            }
            if (!CheckTextLength(txtUser, 6))
            {
                ColorControls(txtUser, redBrush, whiteBrush);
            }
            if (!CheckTextLength(txtProfile, 6))
            {
                ColorControls(txtProfile, redBrush, whiteBrush);
            }
        }

        private SolidColorBrush GetSolidColorBrush(Color color)
        {
            return new SolidColorBrush(color);
        }

        private void ColorControls(Control ui, SolidColorBrush backcolor = null, SolidColorBrush forecolor = null)
        {
            backcolor = backcolor ?? new SolidColorBrush(Colors.White);
            forecolor = forecolor ?? new SolidColorBrush(Colors.Black);

            ui.Background = backcolor;
            ui.Foreground = forecolor;
        }

        private void AddDataGridColumns(string[] cols)
        {
            foreach (string s in cols)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = s;
                textColumn.Binding = new Binding(s);
                dataGrid.Columns.Add(textColumn);
            }
        }

        private bool CheckTextLength(Control control, int length)
        {
            if (control.GetType() == typeof(TextBox))
            {
                TextBox textBox = (TextBox)control;

                if (textBox.Text.Length > length)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (control.GetType() == typeof(PasswordBox))
            {
                PasswordBox textBox = (PasswordBox)control;

                if (textBox.Password.Length > length)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.ConnectionBuilder.SelectEntry("role", new string[] { "rolename" });
            Console.WriteLine(ConnectionBuilder.Result.col.ToString());
            ConnectionBuilder.Result.col.RemoveRange(0, 1);
            this.ConnectionBuilder.Result.col.ForEach(x => comboBox.Items.Add(x));
            comboBox.SelectedIndex = 1;
        }

        private string GetHash(string text)
        {
            byte[] byteStream = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(byteStream);
            string String = string.Empty;
            foreach (byte x in hash)
                String += String.Format("{0:x2}", x);
            return String;
        }

        private void CmdCreateEntry_Click(object sender, RoutedEventArgs e)
        {
            if (CheckTextLength(txtProfileAdd, 6))
            {
                Dictionary<string, object> tmp = new Dictionary<string, object>
                {
                    { "profilename", txtProfileAdd.Text },
                    { "user_id", userId },
                    { "additional_information", CreateJSON() }
                };

                this.ConnectionBuilder.CreateEntry("profile", tmp);
            }
            else
            {
                if (!CheckTextLength(txtProfileAdd, 6))
                {
                    ColorControls(txtProfileAdd, GetSolidColorBrush(Colors.Red), GetSolidColorBrush(Colors.White));
                }
            }
        }

        private void CmdAddData_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxID < 6)
            {
                TextBox txtbox = new TextBox();
                txtbox.Text = "textbox1";
                txtbox.Height = 24;
                txtbox.Name = "Data" + TextBoxID;
                txtbox.TabIndex = TextBoxID + txtProfileAdd.TabIndex;
                TextBoxID++;

                Label lbl = new Label();
                lbl.Content = "Data" + TextBoxID;
                lbl.Height = 24;

                txtbox.TextChanged += Txtbox_TextChanged;

                this.stkLabels.Children.Add(lbl);
                this.stkTextBox.Children.Add(txtbox);

                scrlText.Height = 24 * stkTextBox.Children.Count;
                scrlLabel.Height = 24 * stkLabels.Children.Count;
            }
        }

        private void Txtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Label lbl = (Label)stkLabels.Children[stkTextBox.Children.IndexOf((UIElement)e.Source)];
            TextBox txt = (TextBox)e.Source;
            lbl.Content = txt.Text;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) { }

        private string CreateJSON()
        {
            string result = "{";
            int count = 0;
            foreach (TextBox txt in stkTextBox.Children)
            {
                result += "\"Data" + count++ + "\": " + "\"" + txt.Text + "\",";
            }
            result += "}";
            return result;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Action method = Exit;
            Action action = delegate ()
            {
                Console.WriteLine("You are cool.");
                dialogWindow.Close();
                darkwindow.Close();
            };
            GetDialogWindow(new string[] { "Yes", "No" }, "Do you really want to exit the application?", method, action).ShowDialog();
        }

        private DialogWindow GetDialogWindow(string[] buttonText, string text, Action actionLeft, Action actionRight)
        {
            try
            {
                darkwindow = new Window()
                {
                    Background = Brushes.Black,
                    Opacity = 0.4,
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    WindowState = WindowState.Maximized,
                    Topmost = true
                };
                darkwindow.Show();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            this.dialogWindow = new DialogWindow();
            dialogWindow.Owner = this;
            dialogWindow.Topmost = true;
            dialogWindow.WindowStyle = WindowStyle.None;
            dialogWindow.HorizontalAlignment = HorizontalAlignment.Center;
            dialogWindow.VerticalAlignment = VerticalAlignment.Center;
            dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialogWindow.Owner = Application.Current.MainWindow;
            dialogWindow.Dialog.btnLeft.Content = buttonText[0];
            dialogWindow.Dialog.btnRight.Content = buttonText[1];
            dialogWindow.Dialog.txtBlock.Text = text;
            dialogWindow.Dialog.btnLeft.Click += (s, e) => actionLeft();
            dialogWindow.Dialog.btnRight.Click += (s, e) => actionRight();
            return dialogWindow;
        }

        private void Exit()
        {
            foreach (Window window in App.Current.Windows)
            {
                window.Close();
            }
        }
    }
}

namespace M120_EasyEZ.Tools
{

    [ValueConversion(typeof(string), typeof(string))]
    public class RatioConverter : MarkupExtension, IValueConverter
    {
        private static RatioConverter _instance;

        public RatioConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { // do not let the culture default to local to prevent variable outcome re decimal syntax
            double size = System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return size.ToString("G0", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { // read only converter...
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new RatioConverter());
        }

    }
}
