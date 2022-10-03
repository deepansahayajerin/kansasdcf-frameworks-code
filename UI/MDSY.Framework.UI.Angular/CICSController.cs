using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Globalization;
using MDSY.Framework.WCF.Interfaces;
using MDSY.Framework.Control.IDMS;
using MDSY.Framework.Interfaces;
using MDSY.Framework.Core;
using MDSY.Utilities.Security;

namespace MDSY.Framework.UI.Angular
{
    [Serializable]
    public class CICSController : Page
    {
        protected const string PAGE_CLASS = "pageClass";

        #region Private Members
        private string _styleSheet = string.Empty;
        private string _WCFAddress = string.Empty;
        private string _titleBar = string.Empty;
        private string _keyPressed = string.Empty;
        private string _currentForm = string.Empty;
        private string _currentControl = string.Empty;
        private string _mdtFlags = string.Empty;
        private string _eofFlags = string.Empty;
        private string _rowColumnPosition = string.Empty;
        private int _currentPosition = 0;
        private string KeyPressedValue;
        protected ICICSService _sessionServiceClient;
        private List<IAterasServiceItem> _serviceControlsList;
        //private Configuration _internalConfigurationManager;
        private bool isDynamicPopup = false;
        private Panel modalPanel;
        private ModalPopupExtender modalPopup;
        private TextBox focusedTextBox;
        private string UserMessageTop = ConfigurationManager.AppSettings["UserMessageTop"].noNull("hide");
        private string SystemName = ConfigurationManager.AppSettings["SystemName"].noNull("Online");
        private static Dictionary<string, string> _keyOverrides;
        private string overrideMessage;
        private string _warningMessage = null;

        private string _keyPressedValue;
        private TextBox _focusedTextBox;
        private CICSController _nextPage = null;
        private bool _isDynamicPopup = false;
        private bool _isDynamicMap = false;
        private Panel _modalPanel;
        private string _windowName = null;
        protected CICSController _lastPage = null;

        ///////////////////////////////////////////////////////////////////////////////////////
        /// DynamicMap related members
        ///////////////////////////////////////////////////////////////////////////////////////
        private bool _isFieldDefinitionOnly = true;

        ///////////////////////////////////////////////////////////////////////////////////////
        /// Java specific class members
        ///////////////////////////////////////////////////////////////////////////////////////
        private HtmlGenericControl _controlDiv = new HtmlGenericControl("controlDiv");
        private Panel _divPFKeyLegend = new Panel("divPFKeyLegend");
        #endregion

        #region Protected Properties
        public List<IAterasServiceItem> ServiceControlsList
        {
            get
            {
                if (_serviceControlsList == null)
                    _serviceControlsList = new List<IAterasServiceItem>();
                return _serviceControlsList;
            }
            set { _serviceControlsList = value; }
        }

        protected string StyleSheet
        {
            get
            {
                try
                {
                    if (_styleSheet == string.Empty)
                    {
                        _styleSheet = ConfigurationManager.AppSettings["StyleSheet"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }

                return _styleSheet;
            }
        }

        protected string KeyPressed
        {
            get
            {
                return _keyPressed;
            }
        }

        protected string WCFAddress
        {
            get
            {
                if (_WCFAddress == string.Empty)
                {
                    _WCFAddress = ConfigurationManager.AppSettings["WCFAddress"].ToString();
                }
                return _WCFAddress;
            }
        }

        protected string CurrentForm
        {
            get { return _currentForm; }
            set { _currentForm = value; }
        }

        protected string MdtFlags
        {
            get { return _mdtFlags; }
            set { _mdtFlags = value; }
        }

        #endregion

        #region Protected methods

        protected void Page_Load(/*object sender, System.EventArgs e*/)
        {

            _sessionServiceClient = (ICICSService)_session["sessionService"];
            if (!Security.IsSystemAvaliable(SystemName))
            {
                TransferToNewPage("logoff");
            }
            if (/*!IsPostBack || */ _sessionServiceClient == null)
            {
                //string name = Context.User.Identity.Name;
                PrepareKeyOverrides();

                if (_sessionServiceClient == null)
                // Process application start
                {
                    CreateServiceConnection();
                }
                else
                {
                    // Process Transfer from another form
                    GetPreviousPageData();
                }
            }
            else
            {
                // Process current page information
                //_keyPressed = Request.Form["__EVENTKEYPRESS"];
                //_mdtFlags = Request.Form["__MDTFLAGS"];
                //_rowColumnPosition = Request.Form["__EVENTCURSORPOS"];
                //_eofFlags = Request.Form["__EOFFLAGS"];
                //PageLoadEvent(sender, e);
                if (_sessionServiceClient == null)
                {
                    _sessionServiceClient = (ICICSService)_session["sessionService"];
                }

                _serviceControlsList = _sessionServiceClient.GetValues();

                ProcessServiceCall(false);
            }

            SetPageValuesFromServiceData();

        }

        //protected override void OnPreRender(EventArgs e)
        //{
        //	try
        //	{
        //		ClientScript.RegisterHiddenField("__EVENTKEYPRESS", " ");
        //		ClientScript.RegisterHiddenField("__EVENTCURSORPOS", "   ");
        //		ClientScript.RegisterHiddenField("__EEOFFLAGS", _eofFlags);
        //		ClientScript.RegisterHiddenField("__MDTFLAGS", _mdtFlags);
        //		base.OnPreRender(e);
        //	}
        //	catch (Exception ex)
        //	{
        //		Session["Error"] = new Exception(ex.Message);
        //		Response.Redirect("Error.aspx");
        //	}
        //}

        private void OnInit(/*EventArgs e*/)
        {

            if (_session == null)
                return;
            else
            {
                _lastPage = (CICSController)_session[PAGE_CLASS];
                _sessionServiceClient = (ICICSService)_session["sessionService"];
            }

            if (_sessionServiceClient != null)
            {
                if (CurrentForm == "Error")
                {
                    ServiceControlsList.Add(new CICSServiceItemKey("Error", "QUIT", string.Empty, string.Empty, true, string.Empty));
                    ServiceControlsList.Add(new CICSServiceItemControl("Message", (string)_session["ErrorMessage"], true, 80, "Red"));
                }
                else
                    try
                    {
                        ServiceControlsList = _sessionServiceClient.GetValues();
                    }
                    catch (System.ServiceModel.CommunicationException)
                    {
                        //Server.Transfer(String.Concat("~/Error.aspx?message=", HttpUtility.UrlEncode("Communication with the server is lost.")));
                        throw new Exception("not implemented");
                    }
                    catch (Exception exc)
                    {
                        ServiceControlsList.Add(new CICSServiceItemKey("Error", "QUIT", string.Empty, string.Empty, true, string.Empty));
                        ServiceControlsList.Add(new CICSServiceItemControl("Message", exc.Message, true, 80, "Red"));
                    }

                if (ServiceControlsList != null)
                {

                    CICSServiceItemKey serviceKey = (CICSServiceItemKey)ServiceControlsList.Find(o => o is CICSServiceItemKey);
                    if (serviceKey != null && serviceKey.Name == "Dynamic")
                    {
                        SetUpDynamicControls();
                    }
                }
            }

        }

        virtual protected void PageLoadEvent(object sender, System.EventArgs e)
        { }

        protected static string GetKeyFromAidByte(string strCode)
        {
            switch (strCode)
            {
                case "1":
                    return "PF1";
                case @"'":
                    return "ENTER";
                case "2":
                    return "PF2";
                case "3":
                    return "PF3";
                case "4":
                    return "PF4";
                case "5":
                    return "PF5";
                case "6":
                    return "PF6";
                case "7":
                    return "PF7";
                case "8":
                    return "PF8";
                case "9":
                    return "PF9";
                case ":":
                    return "PF10";
                case "#":
                    return "PF11";
                case "@":
                    return "PF12";
                case "A":
                    return "PF13";
                case "B":
                    return "PF14";
                case "C":
                    return "PF15";
                case "D":
                    return "PF16";
                case "E":
                    return "PF17";
                case "F":
                    return "PF18";
                case "G":
                    return "PF19";
                case "H":
                    return "PF20";
                case "I":
                    return "PF21";
                case "[":
                    return "PF22";
                case ".":
                    return "PF23";
                case "<":
                    return "PF24";
                case "%":
                    return "PA1";
                case ">":
                    return "PA2";
                case ",":
                    return "PA3";
                case "_":
                    return "CLEAR";
                case "|":
                    return "ESCAPE";
                default:
                    return " ";
            }
        }

        protected virtual IControl GetTableDiv()
        {
            return null;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Set up Service channel and send first request
        /// </summary>
        private void CreateServiceConnection()
        {
            _sessionServiceClient = new ChannelFactory<ICICSService>("NetTcpBinding_ICICSService").CreateChannel();
            if (_sessionServiceClient == null)
            {
                throw new Exception(String.Concat("Unable to connect to WCF service: ", WCFAddress));
            }
            else if (!_sessionServiceClient.Test())
            {
                _sessionServiceClient = new ChannelFactory<ICICSService>("NetTcpBinding_ICISCSService").CreateChannel();
            }

            if (_sessionServiceClient == null)
            {
                throw new Exception(String.Concat("Unable to connect to WCF service: ", WCFAddress));
            }

            _sessionServiceClient.Initialize((string)_session["LoginUserID"], (string)_session["TermID"]);

            IClientChannel contextChannel = _sessionServiceClient as IClientChannel;
            //contextChannel.OperationTimeout = TimeSpan.FromMinutes(60);

            // Update Session 
            _session["sessionService"] = _sessionServiceClient;

            ProcessServiceCall(true);
        }

        /// <summary>
        /// Get the serviceControls from the last page where we transferred from
        /// </summary>
        private void GetPreviousPageData()
        {
            if (_session[PAGE_CLASS] is CICSController)
            {
                CICSController lastPage = (CICSController)_session[PAGE_CLASS];
                _serviceControlsList = lastPage.ServiceControlsList;
            }
            CICSServiceItemKey serviceKey = (CICSServiceItemKey)_serviceControlsList.Find(o => o is CICSServiceItemKey);
            if (serviceKey != null && serviceKey.Name.ToUpper() != "ERROR")
            {
                overrideMessage = serviceKey.MessagePosition;
                if (serviceKey.KeyPressed == "QUIT")
                {
                    _sessionServiceClient = null;
                    _session["sessionService"] = null;
                }

                if (serviceKey.CurrentControl != string.Empty)
                {
                    if (!int.TryParse(serviceKey.CurrentPosition, out _currentPosition))
                        _currentPosition = 0;
                    _currentControl = serviceKey.CurrentControl;
                }
            }
        }

        /// <summary>
        /// Send Service request and process return data
        /// </summary>
        private void ProcessServiceCall(bool creatingConnection)
        {
            SetServiceDataFromPageValues();

            try
            {
                _sessionServiceClient.Run();
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                //Server.Transfer(String.Concat("~/Error.aspx?message=", HttpUtility.UrlEncode("Server is unavailable.  Please contact system administrator. " + ex.Message)));
                throw new Exception("not implemented");
            }
            catch (Exception ex)
            {
                SendError(String.Concat("Problem with WCF Service Call: ", ex.Message));
            }

            _serviceControlsList = _sessionServiceClient.GetValues();
            //Check for Page transfer
            CICSServiceItemKey serviceKey = (CICSServiceItemKey)_serviceControlsList.Find(o => o is CICSServiceItemKey);
            if (serviceKey != null)
            {
                if (!string.IsNullOrEmpty(serviceKey.CurrentControl) && string.IsNullOrEmpty(serviceKey.CurrentPosition))
                {
                    TextBox pageTextBox = (TextBox)FindControlRecursive(serviceKey.CurrentControl);
                    if (pageTextBox != null)
                    {
                        serviceKey.CurrentPosition = pageTextBox.Attributes["idx"];
                    }
                }

                overrideMessage = serviceKey.MessagePosition;

                if (serviceKey.KeyPressed == "QUIT")
                {
                    if (serviceKey.Name.ToUpper() == "ERROR")
                    {
                        CICSServiceItemControl serviceControl = null;
                        serviceControl = (CICSServiceItemControl)_serviceControlsList.Find(c => c is CICSServiceItemControl);
                        SendError(serviceControl.Text);
                    }
                    _sessionServiceClient = null;
                    _session["sessionService"] = null;
                    string landingPage = ConfigurationManager.AppSettings["LandingPage"];
                    if (string.IsNullOrEmpty(landingPage))
                        landingPage = "default";
                    RedirectToNewPage("~/Default.aspx");
                    //TransferToNewPage(landingPage);
                }

                if (serviceKey.KeyPressed == "ESCAPE")
                {
                    _sessionServiceClient = null;
                    _session["sessionService"] = null;
                    string landingPage = ConfigurationManager.AppSettings["LandingPage"];
                    if (string.IsNullOrEmpty(landingPage))
                        landingPage = "default";
                    TransferToNewPage(landingPage);
                }

                if (serviceKey.Name == "Dynamic")
                    isDynamicPopup = true;
                else
                {
                    isDynamicPopup = false;
                    if (serviceKey.Name != CurrentForm)
                    {
                        if (!(serviceKey.Name.Contains("EntryPoint:") && CurrentForm == "Error"))
                        {
                            if (serviceKey.Name.Contains("EntryPoint:"))
                            {
                                SendError("Unable to find " + serviceKey.Name);
                            }
                            else
                            {
                                TransferToNewPage(serviceKey.Name);
                            }
                        }
                    }
                }


                if (serviceKey.CurrentControl != string.Empty || serviceKey.CurrentPosition != string.Empty)
                {
                    if (!int.TryParse(serviceKey.CurrentPosition, out _currentPosition))
                    {
                        _currentPosition = 0;
                        serviceKey.CurrentControl = serviceKey.CurrentPosition;
                    }
                    _currentControl = serviceKey.CurrentControl;

                }

            }

        }

        /// <summary>
        /// Copy the contents of the Web Page contols to Service Controls list
        /// </summary>
        private void SetServiceDataFromPageValues()
        {
            ProcessCursorPosition();

            // Find and set Item Key
            CICSServiceItemKey serviceKey = (CICSServiceItemKey)ServiceControlsList.Find(o => o is CICSServiceItemKey);
            KeyPressedValue = GetKeyFromAidByte(KeyPressed);
            //Check for Key Overrides
            CheckForKeyOverride();
            if (serviceKey != null)
            {
                serviceKey.KeyPressed = KeyPressedValue;
            }
            else
            {
                ServiceControlsList.Add(new CICSServiceItemKey(CurrentForm, KeyPressedValue, string.Empty, string.Empty));
            }
            if (isDynamicPopup)
            {
                //   GetDynamicValues();
            }
            // Update control items
            else
            {
                for (int ctr = 0; ctr < _serviceControlsList.Count; ctr++)
                {
                    if (_serviceControlsList[ctr] is CICSServiceItemControl)
                    {
                        if (((CICSServiceItemControl)_serviceControlsList[ctr]).Name == "WarningMessagePopup")
                        {
                            _serviceControlsList.Remove(_serviceControlsList[ctr]);
                            break;
                        }
                    }
                }

                for (int ctr = 1; ctr < _serviceControlsList.Count; ctr++)
                //foreach (IAterasServiceItem serviceItem in _serviceControlsList)
                {

                    if (_serviceControlsList[ctr] is CICSServiceItemControl)
                    {
                        CICSServiceItemControl serviceControl = (CICSServiceItemControl)_serviceControlsList[ctr];

                        if (_currentPosition == ctr)
                        {
                            serviceKey.CurrentControl = serviceControl.Name;
                            serviceKey.CurrentPosition = _currentPosition.ToString();
                        }

                        if (!serviceControl.ReadOnly)
                        {
                            if (ctr - 1 >= _mdtFlags.Length) continue;

                            if (_mdtFlags[ctr - 1] == 'Y')
                            {
                                serviceControl.Modified = true;
                                TextBox pageTextBox = (TextBox)FindControlRecursive(serviceControl.Name);

                                if (pageTextBox == null)
                                    continue;

                                if (serviceControl.ForceUpperCase)
                                    serviceControl.Text = pageTextBox.Text.ToUpper();
                                else
                                    serviceControl.Text = pageTextBox.Text;

                                if (serviceControl.FillCharacter != ' ')
                                {
                                    serviceControl.Text = serviceControl.Text.TrimEnd(serviceControl.FillCharacter);
                                    //todo: Add logic for numeric type fields
                                }
                            }
                            else
                                serviceControl.Modified = false;
                        }
                    }
                }
            }

            try
            {
                _sessionServiceClient.SetValues(ServiceControlsList);
            }
            catch (Exception ex)
            {
                SendError(string.Concat("***** Problem retrieving data from WCF service call! ***** \n", ex.Message));
            }
        }

        /// <summary>
        /// Copy the values and attributes of the Service controls to the Web Page Textboxes
        /// </summary>
        private void SetPageValuesFromServiceData()
        {
            if (overrideMessage == "Top")
                UserMessageTop = "true";
            //if (UserMessageTop != "hide" && !this.ClientScript.IsStartupScriptRegistered("UserMessageTop"))
            //{
            //	Page.ClientScript.RegisterStartupScript(Page.GetType(), "UserMessageTop", "var UserMessageTop = {0};".Format((object)UserMessageTop), true);
            //}

            if (string.IsNullOrEmpty(_currentControl) && _currentPosition < 1)
                _currentPosition = 1;

            if (isDynamicPopup)
            {
                SetUpDynamicControls();
            }

            focusedTextBox = null;
            string focusedControlID = null;
            StringBuilder mdtFlags = new StringBuilder();
            foreach (IAterasServiceItem serviceItem in _serviceControlsList)
            {
                if (serviceItem is CICSServiceItemControl)
                {

                    CICSServiceItemControl serviceControl = (CICSServiceItemControl)serviceItem;

                    if (serviceControl.Name == "WarningMessagePopup")
                    {
                        _warningMessage = serviceControl.Text;
                        continue;
                    }

                    TextBox pageTextBox = (TextBox)FindControlRecursive(serviceControl.Name);
                    if (pageTextBox == null)
                        continue;

                    pageTextBox.Attributes.Remove("activeField");
                    //if (serviceControl.Style.Trim() != string.Empty)
                    if (serviceControl.ReadOnly)
                    {
                        pageTextBox.CssClass = string.Concat("TEXTBOX PROTECTED", serviceControl.Style.TrimEnd());
                    }
                    else
                    {
                        pageTextBox.CssClass = string.Concat("TEXTBOX UNPROTECTED", serviceControl.Style.TrimEnd());
                        pageTextBox.CssClass = pageTextBox.CssClass.Replace("TEXTBOX UNPROTECTED BRIGHT", "TEXTBOXUNPROTECTEDBRIGHT");
                    }

                    if (serviceControl.ForceUpperCase && !serviceControl.ReadOnly && serviceControl.Text != null)
                        serviceControl.Text = serviceControl.Text.ToUpper();

                    if (serviceControl.Text == null)
                        pageTextBox.Text = string.Empty;
                    else
                        pageTextBox.Text = serviceControl.Text.Replace('\0', ' '); //.TrimNotNull(); // PAUL - to fix Chromes default of selecting field...

                    if (serviceControl.Modified)
                        mdtFlags.Append("Y");
                    else
                        mdtFlags.Append("N");

                    if (serviceControl.ReadOnly)
                    {
                        pageTextBox.ReadOnly = serviceControl.ReadOnly;
                        pageTextBox.TabIndex = -1;
                    }
                    else
                    {
                        pageTextBox.ReadOnly = false;
                        if (serviceControl.FillCharacter != ' ')
                        {
                            //todo: Add logic for numeric type fields
                            pageTextBox.Text = pageTextBox.Text.TrimEnd().PadRight(pageTextBox.MaxLength, serviceControl.FillCharacter);
                        }
                        if (_currentPosition > 0)
                        {
                            if (pageTextBox.TabIndex == _currentPosition)
                            {
                                focusedTextBox = pageTextBox;
                            }
                            else if ((focusedTextBox == null && string.IsNullOrEmpty(focusedControlID)) || (!string.IsNullOrEmpty(focusedControlID) && pageTextBox.Id == focusedControlID))
                            {
                                focusedTextBox = pageTextBox;
                            }
                        }
                        else if (_currentControl != string.Empty)
                        {
                            if (pageTextBox.Id == _currentControl)
                                focusedTextBox = pageTextBox;
                        }

                    }

                    if (serviceControl.Autoskip)
                        pageTextBox.Attributes.Add("autoskip", "true");
                }
                else if (serviceItem is CICSServiceItemKey)
                {
                    focusedControlID = ((CICSServiceItemKey)serviceItem).CurrentControl;
                }
                //}
                _mdtFlags = mdtFlags.ToString();
                _eofFlags = _mdtFlags;
            }

            if (_warningMessage != null)
            {
                SetUpModalMessageBox("Warning", _warningMessage);
            }

            // Set Cursor 
            if (focusedTextBox != null && modalPopup == null)
            {
                focusedTextBox.Focus();
                focusedTextBox.Attributes.Add("activeField", "true");
            }
            else if (!string.IsNullOrEmpty(focusedControlID))
            {
                TextBox csrTextBox = (TextBox)FindControlRecursive(focusedControlID);
                if (csrTextBox != null)
                {
                    csrTextBox.Focus();
                    csrTextBox.Attributes.Add("activeField", "true");
                }
            }

            //if (isDynamicPopup || _warningMessage != null)
            //{
            //	ShowPopUp();
            //	SetPopupStartScript();
            //}
            //else
            //{
            //	if (modalPopup != null)
            //	{
            //		HidePopUp();
            //	}
            //}

            try
            {
                if (ServiceControlsList[0].Name != "Error")
                    _sessionServiceClient.SetValues(ServiceControlsList);
            }
            catch (Exception ex)
            {
                SendError(string.Concat("***** Problem retrieving data from BL service call! ***** \n", ex.Message));
            }
        }

        /// <summary>
        /// For dynamic forms, set up dynamic controls
        /// </summary>
        private void SetUpDynamicControls()
        {
            if (modalPopup != null)
                return;

            focusedTextBox = null;
            int xOffSet = 0; int yOffSet = 0;
            decimal xBound = 0; decimal yBound = 0;

            HtmlGenericControl controlDiv = new HtmlGenericControl("DIV");
            controlDiv.Id = "popupDivControls";


            int textBoxHeight = 18;
            float xMultiplier = 9f;
            float yMultiplier = 18;
            float widthMultiplier = 9f;

            int zIndex = ServiceControlsList.Count + 1;
            int idxCtr = 1; short tabCtr = 1;

            StringBuilder mdtFlags = new StringBuilder();
            CICSServiceItemControl CICSServiceItemControl = null;
            TextBox textBox = null;
            decimal tLeft = 0;
            decimal tWidth = 0;
            decimal tTop = 0;
            textBox = new TextBox();
            textBox.Attributes.Add("ANVKey", KeyPressedValue);
            textBox.Attributes.Add("type", "hidden");
            controlDiv.Controls.Add(textBox.Id, textBox);


            foreach (var value in ServiceControlsList)
            {
                if (value is CICSServiceItemControl)
                {
                    CICSServiceItemControl = (CICSServiceItemControl)value;

                    textBox = new TextBox();
                    if (CICSServiceItemControl.Name == "Message")
                        continue;
                    else
                        textBox.Id = CICSServiceItemControl.Name;

                    // position

                    tLeft = Convert.ToDecimal(CICSServiceItemControl.NaturalLocationX * xMultiplier);
                    tWidth = Convert.ToDecimal(CICSServiceItemControl.Length * widthMultiplier);
                    tTop = Convert.ToDecimal(CICSServiceItemControl.NaturalLocationY * yMultiplier);

                    textBox.Style.Add(HtmlTextWriterStyle.Left, (tLeft) + "px");
                    textBox.Style.Add(HtmlTextWriterStyle.Top, (tTop) + "px");
                    textBox.Style.Add(HtmlTextWriterStyle.Width, (tWidth) + "px");
                    textBox.Style.Add(HtmlTextWriterStyle.Position, "absolute");
                    textBox.Style.Add(HtmlTextWriterStyle.ZIndex, zIndex.ToString());
                    zIndex--;

                    // size
                    textBox.Columns = CICSServiceItemControl.Length;
                    textBox.MaxLength = CICSServiceItemControl.Length;
                    textBox.Height = textBoxHeight;
                    // font and diplay 
                    //textBox.Font.Name = "Courier New";

                    textBox.ReadOnly = CICSServiceItemControl.ReadOnly;

                    // get terminal window bounds
                    if (xBound < (tLeft + tWidth))
                        xBound = (tLeft + tWidth);
                    if (yBound < (tTop + textBoxHeight))
                        yBound = (tTop + textBoxHeight);
                    textBox.Attributes.Add("runat", "server");
                    textBox.Attributes.Add("idx", idxCtr.ToString());

                    if (!textBox.ReadOnly)
                    {
                        if (CICSServiceItemControl.Name == "pndCONFIRM_DELETE") // or attributes set...
                        {
                            textBox.Attributes.Add("data-AterasNaturalValidator", "DT=A001~AD=LTE~EM=X~TG=YN");
                        }
                        if (focusedTextBox == null) { focusedTextBox = textBox; }
                        if (_currentPosition > 0)
                        {
                            if (textBox.TabIndex == _currentPosition)
                            {
                                focusedTextBox = textBox;
                            }
                            else if (focusedTextBox == null)
                            {
                                focusedTextBox = textBox;
                            }
                        }
                        else if (_currentControl != string.Empty)
                        {
                            if (textBox.Id == _currentControl)
                                focusedTextBox = textBox;
                        }

                        textBox.TabIndex = tabCtr;
                        tabCtr++;
                        textBox.Text = "";
                    }
                    else
                        textBox.TabIndex = -1;

                    bool duplicateFound = false;
                    foreach (IControl controlItem in controlDiv.Controls.Values)
                    {
                        if (controlItem.Id == textBox.Id)
                        {
                            duplicateFound = true;
                            break;
                        }
                    }

                    if (!duplicateFound)
                    {
                        controlDiv.Controls.Add(textBox.Id, textBox);
                    }

                    mdtFlags.Append("N");
                    idxCtr++;
                }
            }

            MdtFlags = mdtFlags.ToString();



            #region controlPanel
            CICSServiceItemKey serviceKey = (CICSServiceItemKey)ServiceControlsList[0];
            if (!string.IsNullOrEmpty(serviceKey.SaveArea))
            {
                if (serviceKey.SaveArea.Contains("PositionRow="))
                {
                    yOffSet = Convert.ToInt32(serviceKey.SaveArea.Substring(serviceKey.SaveArea.IndexOf("PositionRow=") + 12, 2)) * 18;
                }
                if (serviceKey.SaveArea.Contains("PositionColumn="))
                {
                    xOffSet = Convert.ToInt32(serviceKey.SaveArea.Substring(serviceKey.SaveArea.IndexOf("PositionColumn=") + 15, 2)) * 9;
                }
                if (serviceKey.SaveArea.Contains("SizeRows="))
                {
                    yBound = Convert.ToInt32(serviceKey.SaveArea.Substring(serviceKey.SaveArea.IndexOf("SizeRows=") + 9, 2)) * 18;
                }
                if (serviceKey.SaveArea.Contains("SizeColumns="))
                {
                    xBound = Convert.ToInt32(serviceKey.SaveArea.Substring(serviceKey.SaveArea.IndexOf("SizeColumns=") + 12, 2)) * 9;
                }
            }
            overrideMessage = serviceKey.MessagePosition;
            IControl tableDiv = (IControl)FindControlRecursive("TablePanel");

            //Create panel for displaying controls 
            modalPanel = new Panel();
            modalPanel.Id = "ModalPanel";
            modalPanel.Width = xBound + "px";
            modalPanel.Height = yBound + "px";
            modalPanel.CssClass = "ModalPanel";
            modalPanel.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            modalPanel.BorderWidth = "1px";
            modalPanel.Controls.Add(controlDiv.Id, controlDiv);

            // Create Button Div
            HtmlGenericControl buttonDiv = new HtmlGenericControl("DIV");
            buttonDiv.Id = "ButtonDiv"; buttonDiv.Style.Add(HtmlTextWriterStyle.Display, "None");
            Button okBTN = new Button(); okBTN.Id = "OKButton"; okBTN.Text = "OK";
            buttonDiv.Controls.Add(okBTN.Id, okBTN);
            Button modBTN = new Button(); modBTN.Id = "ModalButton"; modBTN.Style.Add(HtmlTextWriterStyle.Display, "None");
            buttonDiv.Controls.Add(modBTN.Id, okBTN);

            modalPanel.Controls.Add(buttonDiv.Id, buttonDiv);

            // Create ModalPopupExtender
            modalPopup = new ModalPopupExtender();
            modalPopup.Id = "mpe";
            modalPopup.PopupControlId = "ModalPanel";
            modalPopup.TargetControlId = "ModalButton";
            modalPopup.BehaviorId = "ModalPopUpBehavior";
            modalPopup.DropShadow = true;
            modalPopup.OkControlId = "OKButton";
            modalPopup.BackgroundCssClass = "ModalPopup";
            modalPopup.X = xOffSet;
            modalPopup.Y = yOffSet;

            // Add ModalPopUpExtender to dynamic Panel
            modalPanel.Controls.Add(modalPopup.Id, modalPanel);

            // Add dynamic Panel to Page Panel
            tableDiv.Controls.Add(modalPanel.Id, modalPanel);

            #endregion
        }

        private void SetUpModalMessageBox(string caption, string message)
        {
            Panel tableDiv = (Panel)FindControlRecursive("TablePanel");

            int tableDivHeight = 0;
            int.TryParse(tableDiv.Style[HtmlTextWriterStyle.Height].Replace("px", ""), out tableDivHeight);
            int tableDivWidth = 0;
            int.TryParse(tableDiv.Style[HtmlTextWriterStyle.Width].Replace("px", ""), out tableDivWidth);

            Button modBTN = new Button();
            modBTN.Id = "HiddenModalMessageBoxButton";
            modBTN.Style.Add(HtmlTextWriterStyle.Display, "None");
            tableDiv.Controls.Add(modBTN.Id, modBTN);

            Panel modalPanel = new Panel();
            modalPanel.Id = "ModalPanel";
            modalPanel.BorderWidth = "0px";
            modalPanel.BorderStyle = BorderStyle.None;
            modalPanel.CssClass = "ModalMessageBoxPanel";

            HtmlGenericControl modalPopupDiv = new HtmlGenericControl("div");
            modalPopupDiv.Id = "ModalPopupDiv";
            modalPopupDiv.Attributes["class"] = "ModalMessageBox";

            HtmlGenericControl modalPopupHeader = new HtmlGenericControl("div");
            modalPopupHeader.Id = "ModalPopupHeader";
            modalPopupHeader.Attributes["class"] = "ModalMessageBoxHeader";
            modalPopupHeader.InnerHtml = caption;
            modalPopupDiv.Controls.Add(modalPopupHeader.Id, modalPopupHeader);

            HtmlGenericControl modalPopupMessage = new HtmlGenericControl("div");
            modalPopupMessage.Id = "ModalPopupMessage";
            modalPopupMessage.Attributes["class"] = "ModalMessageBoxMessage";
            modalPopupMessage.InnerHtml = message;
            modalPopupDiv.Controls.Add(modalPopupMessage.Id, modalPopupMessage);

            HtmlGenericControl okButton = new HtmlGenericControl("input");
            okButton.Id = "btnOKay";
            okButton.Attributes["type"] = "button";
            okButton.Attributes["value"] = "OK";
            okButton.Attributes["class"] = "ModalMessageBoxButton";
            modalPopupDiv.Controls.Add(okButton.Id, okButton);

            HtmlGenericControl cancelButton = new HtmlGenericControl("input");
            cancelButton.Id = "btnCancel";
            cancelButton.Attributes["type"] = "button";
            cancelButton.Attributes["value"] = "Cancel";
            cancelButton.Style.Add(HtmlTextWriterStyle.Display, "None");
            modalPopupDiv.Controls.Add(cancelButton.Id, cancelButton);

            modalPanel.Controls.Add(modalPopupDiv.Id, modalPopupDiv);
            tableDiv.Controls.Add(modalPanel.Id, modalPanel);

            modalPopup = new ModalPopupExtender();
            modalPopup.Id = "ModalMessageBoxExtender";
            modalPopup.PopupControlId = "ModalPanel";
            modalPopup.TargetControlId = "HiddenModalMessageBoxButton";
            modalPopup.BehaviorId = "ModalPopUpBehavior";
            modalPopup.DropShadow = true;
            modalPopup.OkControlId = "btnOKay";
            modalPopup.CancelControlId = "btnCancel";
            modalPopup.BackgroundCssClass = "ModalPopup";

            tableDiv.Controls.Add(modalPopup.Id, modalPopup);
        }

        //private void ShowPopUp()
        //{
        //	Panel modalPanel = (Panel)FindControlRecursive(Page, "ModalPanel");
        //	modalPanel.Visible = true;
        //	modalPopup.Show();

        //}

        //private void HidePopUp()
        //{
        //	modalPanel.Visible = false;
        //	modalPanel = null;
        //	modalPopup.Hide();
        //	modalPopup = null;
        //}

        //private void SetPopupStartScript()
        //{
        //	if (!this.ClientScript.IsStartupScriptRegistered("PopupStartup") && focusedTextBox != null)
        //	{
        //		StringBuilder sb = new StringBuilder();
        //		sb.Append("<script type=\"text/javascript\">");
        //		//var ShowModal = modalPopup.BehaviorID;
        //		sb.Append("Sys.Application.add_load(modalSetup);");
        //		sb.Append("function modalSetup() {\r\n");
        //		sb.Append(String.Format("var modalPopup = $find('{0}');\r\n", modalPopup.BehaviorID));
        //		sb.Append("if (modalPopup != null) modalPopup.add_shown(SetFocusOnControl); }\r\n");
        //		sb.Append("function SetFocusOnControl() {\r\n");

        //		sb.Append(String.Format("$('#{0}').focus();", focusedTextBox.ClientID));
        //		sb.Append("}");
        //		sb.Append(@"</script>");
        //		Page.ClientScript.RegisterStartupScript(Page.GetType(), "PopupStartup", sb.ToString());
        //	}

        //}

        private void ProcessCursorPosition()
        {
            string[] cursorValues = _rowColumnPosition.Split(new char[1] { ',' });
            int.TryParse(cursorValues[0], out _currentPosition);
        }

        /// <summary>
        /// Transfer to new web page
        /// </summary>
        /// <param name="nextPage"></param>
        public void TransferToNewPage(string nextPage)
        {
            _session[PAGE_CLASS] = this;
            _nextPage = new CICSController(nextPage, this, _isDynamicMap, _isDynamicPopup);
        }

        public void RedirectToNewPage(string nextPage)
        {
            throw new Exception("not implemented");
        }

        public void TransferToFirstPage()
        {
            throw new Exception("not implmented");
            //Response.Redirect("~/Default.aspx");
        }

        public void StartNewEntryPoint(string entryPointProgram)
        {
            ServiceControlsList.Clear();
            ServiceControlsList.Add(new CICSServiceItemKey(string.Concat("EntryPoint:", entryPointProgram), string.Empty, string.Empty, string.Empty));

            CreateServiceConnection();

        }

        public void StartNewEntryPoint(string entryPointProgram, string startData)
        {
            ServiceControlsList.Clear();
            ServiceControlsList.Add(new CICSServiceItemKey(string.Concat("EntryPoint:", entryPointProgram), string.Empty, string.Empty, string.Empty));
            ServiceControlsList.Add(new CICSServiceItemControl("StartData", startData, false, 25, "BRIGHT"));
            CreateServiceConnection();

        }

        public void StartNewEntryPoint(string entryPointProgram, string newSessionId, string login)
        {
            ServiceControlsList.Clear();
            ServiceControlsList.Add(new NatServiceItemKey("EntryPoint:" + entryPointProgram, "", "", ""));

            _session = NatSession.GetSession(newSessionId);
            _session["UserName"] = login;
            CreateServiceConnection();
        }

        public void SetServiceSession(ICICSService serviceSession)
        {
            _sessionServiceClient = serviceSession;
        }
        public void LoginUser(string LoginUserID)
        {
            _session["LoginUserID"] = LoginUserID;
            Security.SetUserSessionID(LoginUserID, _session.Id);
            Security.SetUserThreadCount(LoginUserID, 0);
        }

        public void TermID(string TermID)
        {
            _session["TermID"] = TermID;
        }

        /// <summary>
        /// Find Control on web page
        /// </summary>
        /// <param name="root"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected IControl FindControlRecursive(IControl root, string id)
        {
            if (root.Id.Equals(id))
            {
                return root;
            }

            if (root.Controls != null)
            {
                foreach (IControl c in root.Controls.Values)
                {
                    IControl t = FindControlRecursive(c, id);
                    if (t != null)
                    {
                        return t;
                    }
                }
            }

            return null;
        }

        protected IControl FindControlRecursive(string id)
        {
            IControl control = null;
            if (_isDynamicPopup)
            {
                control = ((HtmlGenericControl)_modalPanel.Controls["popupDivControls" + _windowName]).Controls[id];
            }
            else
            {
                if (_mapData != null && _mapData.ContainsKey(id) && _mapData[id] != null)
                {
                    if (_mapData[id] is Label)
                        control = new Label((Dictionary<string, object>)_mapData[id], id);
                    else
                        control = new TextBox((Dictionary<string, object>)_mapData[id], id);

                    _controlDiv.Controls.Add(control.Id, control);
                }
            }
            return control;
        }

        protected IControl FindControlRecursive(string id, bool isLabel)
        {
            IControl control = null;
            if (_isDynamicPopup)
            {
                control = ((HtmlGenericControl)_modalPanel.Controls["popupDivControls"]).Controls[id];
            }
            else
            {
                if (_mapData != null && _mapData.ContainsKey(id) && _mapData[id] != null)
                {
                    if (isLabel)
                        control = new Label((Dictionary<string, object>)_mapData[id], id);
                    else
                        control = new TextBox((Dictionary<string, object>)_mapData[id], id);
                }
                else
                {
                    if (isLabel)
                        control = new Label(id);
                    else
                        control = new TextBox(id);
                }

                _controlDiv.Controls.Add(control.Id, control);
            }
            return control;
        }       /// <summary>
                /// Send error information to the error webpage
                /// </summary>
                /// <param name="message"></param>
        private void SendError(string message)
        {
            ServiceControlsList.Clear();
            ServiceControlsList.Add(new CICSServiceItemKey("Error", "QUIT", string.Empty, string.Empty, true, string.Empty));
            ServiceControlsList.Add(new CICSServiceItemControl("Message", message, true, message.Length, "Red"));
            _session["ErrorMessage"] = message;
            TransferToNewPage("Error");
        }

        private void PrepareKeyOverrides()
        {

            _keyOverrides = new Dictionary<string, string>();
            try
            {
                // AppSettingsSection section = (AppSettingsSection)InternalConfigurationManager.GetSection("keyOverrides");
                System.Collections.Hashtable section = (System.Collections.Hashtable)ConfigurationManager.GetSection("keyOverrides");

                foreach (string key in section.Keys)
                {
                    _keyOverrides.Add(key, section[key].ToString());
                }
            }
            catch
            {
            }
        }

        private void CheckForKeyOverride()
        {
            if (_keyOverrides == null)
            {
                PrepareKeyOverrides();
            }
            if (_keyOverrides.ContainsKey(KeyPressedValue))
            {
                KeyPressedValue = _keyOverrides[KeyPressedValue];
            }
        }
        #endregion

        ///////////////////////////////////////////////////////
        /// Framework.UI.Angular specfic code
        //////////////////////////////////////////////////////

        private CICSController(Dictionary<string, object> parameters) : base(parameters)
        {
            if (_name == "dynamicpopup")
            {
                _isDynamicPopup = true;
                _modalPanel = new Panel(_mapData);
            }
            else
            {
                HtmlGenericControl tablePanel = new HtmlGenericControl("DIV");
                tablePanel.Id = "TablePanel";
                _controlDiv.Controls.Add("TablePanel", tablePanel);
            }

            OnInit();
        }

        private CICSController(string name, CICSController page, bool isDynamicMap, bool isDynamicPopup) : base(name, page)
        {
            _isDynamicMap = isDynamicMap;
            _isDynamicPopup = isDynamicPopup;
            _keyPressedValue = page._keyPressedValue;

            if (isDynamicPopup)
            {
                this._name = "dynamicpopup";
                _modalPanel = new Panel(_mapData);
            }
            else
            {
                HtmlGenericControl tablePanel = new HtmlGenericControl("DIV");
                tablePanel.Id = "TablePanel";
                _controlDiv.Controls.Add("TablePanel", tablePanel);
            }

            OnInit();
        }

        public static CICSController GetPage(Dictionary<string, object> parameters)
        {
            CICSController page = new CICSController(parameters);

            return page;
        }

        public CICSController GetNexPage()
        {
            if (_isDynamicPopup)
            {
                _name = "dynamicpopup";
                return this;
            }
            else if (_nextPage == null)
            {
                _name = _currentForm.ToLower();
                return this;
            }
            else
            {
                _nextPage.GetPreviousPageData();
                return _nextPage;
            }
        }

        public Dictionary<string, object> getResponse()
        {
            if (_sessionServiceClient == null)
            {
                Dictionary<string, object> response = new Dictionary<string, object>();
                response.Add("name", "login");
                return response;
            }

            if (_name == "dynamicmap")
            {
                _isFieldDefinitionOnly = false;
                //SetUpDynamicFields();
            }
            else
                SetPageValuesFromServiceData();

            _keyPressed = " ";
            _rowColumnPosition = "   ";

            return GetControlMap();
        }

        public bool ProcessRequest()
        {
            if (_session == null)
                return false; // unauthorized request

            _sessionServiceClient = (ICICSService)_session["sessionService"];
            if (_sessionServiceClient == null)
                throw new ErrorException("Connection with the server is lost");


            ProcessServiceCall(false);

            return true;
        }

        public string Id { get { return _name; } }

        public Dictionary<string, IControl> Controls
        {
            get { return _controlDiv.Controls; }
        }

        public virtual Dictionary<string, object> GetControlMap()
        {
            Dictionary<string, object> page = new Dictionary<string, object>();

            Dictionary<string, string> mapBaseData = new Dictionary<string, string>();
            mapBaseData.Add("name", _name);
            mapBaseData.Add("windowName", _windowName);
            mapBaseData.Add("currentForm", _currentForm);
            mapBaseData.Add("lastFocus", _focusedTextBox == null ? "" : _focusedTextBox.Id);
            mapBaseData.Add("eventKeyPres", _keyPressed);
            mapBaseData.Add("eventCursorPos", _rowColumnPosition);
            mapBaseData.Add("eeofFlags", _eofFlags);
            mapBaseData.Add("mdtFlags", _mdtFlags);
            mapBaseData.Add("eventTarget", _eventTarget);
            mapBaseData.Add("eventArgument", _eventArgument);
            mapBaseData.Add("activeDivID", _activeDivID);
            mapBaseData.Add("focusedFieldID", _focusedFieldID);
            mapBaseData.Add("homeKeyCode", _homeKeyCode);
            mapBaseData.Add("codeVersion", CodeVersion);
            if (_message != null)
                mapBaseData.Add("message", _message);
            if (_connectionId != null)
                mapBaseData.Add("connectionId", _connectionId);
            if (_isDynamicMap)
            {
                mapBaseData.Add("left", _controlDiv.Style[HtmlTextWriterStyle.Left]);
                mapBaseData.Add("top", _controlDiv.Style[HtmlTextWriterStyle.Top]);
                mapBaseData.Add("width", _controlDiv.Style[HtmlTextWriterStyle.Width]);
                mapBaseData.Add("height", _controlDiv.Style[HtmlTextWriterStyle.Height]);
            }
            page.Add("mapBaseData", mapBaseData);

            Dictionary<string, object> mapData = new Dictionary<string, object>();
            if (_isDynamicPopup)
            {
                mapData.Add(_modalPanel.Id, _modalPanel.GetControlMap());
            }
            else if (_isDynamicMap)
            {
                Dictionary<string, object> controlDiv = new Dictionary<string, object>();
                foreach (IControl control in _controlDiv.Controls.Values)
                    controlDiv.Add(control.Id, control.GetControlMap());
                mapData.Add("controlDiv", controlDiv);

                if (_divPFKeyLegend.Visible)
                {
                    Dictionary<string, object> divPFKeyLegend = new Dictionary<string, object>();
                    foreach (IControl control in _divPFKeyLegend.Controls.Values)
                        divPFKeyLegend.Add(control.Id, control.GetControlMap());
                    mapData.Add("divPFKeyLegend", divPFKeyLegend);
                }
            }
            else
            {
                foreach (IControl control in _controlDiv.Controls.Values)
                    mapData.Add(control.Id, control.GetControlMap());
            }
            page.Add("mapData", mapData);

            return page;
        }

        public string CodeVersion
        {
            get
            {
                string projectVersion = ConfigurationManager.AppSettings["ProjectVersion"];
                return (projectVersion != null)
                    ? ("Version " + projectVersion).PadRight(118)
                    : new string(' ', 118);
            }
        }

        public string Type
        {
            get { return GetType().Name; }
        }
    }
}
