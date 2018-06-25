using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using gembrook.club.framework.classes;
using gembrook.club.framework.controls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Data.Common;
using Telerik.Web.UI;
using gembrook.club.framework.controls.validators;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Security;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Specialized;


namespace gembrook.club.basic_modules.club_admin.website {
	/// <summary>
	///	social_networking_push
	/// </summary>
	public class social_networking_push : BaseModuleControl {

		protected StyleButton save_button;
		protected int _socialNetworkId;
		protected string _socialNetworkName;
		protected string _socialNetworkUrl;
		protected string _socialNetworkValue;
		protected string _socialNetworkIcon;
		protected HiddenField gidHiddenField;
		protected HiddenField fbHiddenField;
		protected HiddenField fbGroupField;
		protected PlaceHolder facebook_options_placeholder;
		protected bool _isEdit;
		protected string _pageName = "Post to a Social Network";
		protected Label active_networks;
		protected Label description_label;
		protected string _shareType;
		protected string _itemId;
		protected TextBox share_description;
		protected string _descriptionText;
		protected PlaceHolder description_placeholder;
		protected CheckBox social_check;
		protected Repeater social_network_repeater;
		protected Label characterCountLabel;
		protected ImageButton networkButton;
		protected ImageButton resetButton;
		protected Label error_label;
		//display variables
		protected string _postType;
		protected string _postDate;
		protected string _postTitle;
		protected bool _networkClicked;
		protected int numChecked;
		protected DateTime _rawDate;
		protected string _hyphen = "&nbsp;-&nbsp;";
		protected string _formattedURL;
		protected string _blogId;
		protected string _postId;
		protected string _facebookString;
		protected string _facebookToken;
		protected string _clubUrl;
		protected string _shareText;
		//token - API variables
		protected string _twitterString;
		protected string _twitterToken;
		protected string _linkedInString;
		protected string _linkedInToken;
		protected TokenFilter _tokenFilter;
		protected string _checkPrefix = "social_check";
		protected string _resetImageUrl = "/images/social/unlock.png";
		protected bool _twitterChecked = false;
		protected bool _faceBookChecked = false;
		protected bool _linkedInChecked = false;
		protected string _linkedInCacheKeyPrefix = "LinkedInKey";
		protected string _facebookCacheKeyPrefix = "FacebookKey";
		protected string _twitterCacheKeyPrefix = "TwitterKey";
		protected string _twitterSecretKeyPrefix = "TwitterSecretKey";
		protected string _linkedInCacheKey;
		protected string _twitterCacheKey;
		protected string _twitterSecretKey;
		protected string _facebookCacheKey;
		protected string _linkedinGroupId;
		protected string _facebookId;
		protected string _requestNonce; //unique identifier attached to each token request/cache key
		protected string _isFacebookGroup;
		protected PlaceHolder popup_placeholder;
		protected string _popupString;
		protected TextBox social_text;

		oAuthTwitter oTwitter = new oAuthTwitter();
		oAuthFacebook oFB = new oAuthFacebook();
		oAuthLinkedIn oLinkedIn = new oAuthLinkedIn();
		oAuthBase2 o2Base = new oAuthBase2();

		public class FbData {
			public string category { get; set; }
			public string name { get; set; }
			public string access_token { get; set; }
			public List<string> perms { get; set; }
			public string id { get; set; }
		}

		public class Paging {
			public string next { get; set; }
		}

		public class RootObject {
			public List<FbData> data { get; set; }
			public Paging paging { get; set; }
		}

		// Page_Load
		private void Page_Load(object sender, EventArgs e) {
			_shareType = Request.QueryString["module"];
			_itemId = Request.QueryString["item_id"];
			_blogId = Request.QueryString["blog_id"];  //Blog Post Id. Blog stored procedure needs blog id and post id for lookup.
			_postId = Request.QueryString["pst"];
			_postDate = Request.QueryString["date"];
			_postTitle = Request.QueryString["post_title"];
			_clubUrl = club.secureDomainName;
			_requestNonce = o2Base.GenerateRandomNumber(); //generates a random number for each page load / request for token if necessary
			resetButton.DataBind();


			if (!IsPostBack) {
				loadSocialDropDown();
				gidHiddenField.Value = _linkedinGroupId;
				fbHiddenField.Value = _facebookId;
				fbGroupField.Value = _isFacebookGroup;
			}
			getFilterItems();
			_formattedURL = formatUrl();
		}


		private string formatUrl() {
			string formattedURL = string.Empty;
			string blogsId = Constants.BlogsModuleId.ToString();
			string photoId = Constants.PhotoPageModuleId.ToString();
			string eventId = Constants.EventsModuleId.ToString();
			string volunteerId = Constants.VolunteeringModuleId.ToString();
			string newsId = Constants.NewsModuleId.ToString();
			string customPageId = Constants.CustomPageModuleId.ToString();

			if (_shareType == blogsId) {
					_postType = "Blog Post";
					formattedURL = "http://" + club.primaryUrl + ClubLink.getHrefText(PageIds.BlogPostViewPageId, club.clubIdString, null, _blogId, null, "pst="+_postId);
			}
			else if (_shareType == photoId) {
					_postDate = string.Empty;
					_hyphen = string.Empty;
					_postType = "Photos";
					formattedURL = "http://" + club.primaryUrl + ClubLink.getHrefText(PageIds.PhotoPageListPageId, club.clubIdString, _itemId, null, null, null);
			}
			else if (_shareType ==customPageId) {
					_postDate = string.Empty;
					_hyphen = string.Empty;
					_postType = "Custom Page";
					formattedURL = "http://" + club.primaryUrl + ClubLink.getHrefText(PageIds.CustomPagePageId, club.clubIdString, _itemId, null, null, null);
			}
			else if (_shareType == eventId) {
					_postType = "Event";
					formattedURL = "http://" + club.primaryUrl + ClubLink.getHrefText(PageIds.EventDetailPageId, club.clubIdString, null, _itemId, null, null);
			}
			else if (_shareType == volunteerId) {
					_postType = "Volunteering Opportunity";
					formattedURL = "http://" + club.primaryUrl + ClubLink.getHrefText(PageIds.VolunteeringOpportunityListPageId, club.clubIdString, null, _itemId, null, null);
			}
			else if (_shareType == newsId) {
					_postType = "News Article";
					formattedURL = "http://" + club.primaryUrl + ClubLink.getHrefText(PageIds.NewsDetailPageId, club.clubIdString, null, _itemId, null, null);
			}
			else {		
				formattedURL = string.Empty;
			}
			return formattedURL;
		}

		//loadInitialValues
		protected void loadSocialDropDown() {
			SqlConnection connection = null;
			connection = new SqlConnection(ConfigurationData.databaseConnectionString);
			try {
				connection.Open();
				DataReader dataReader = new DataReader("club.dbo.get_social_networks_for_club");
				dataReader.addParameter("@club_id", sessionData.clubId);
				dataReader.addParameter("@for_post", 1); //gets only networks which have API support. Currently facebook, twitter and linkedin.
				dataReader.executeAndBindRepeater(social_network_repeater, connection);
			}
			finally {
				if (connection != null) {
					connection.Close();
				}
			}
			
		}

		private enum SocialNetworkLinkReaderFields { SocialNetworkId, Name, NetworkURL, Icon };

		// social_network_link_repeater_ItemCreated
		protected void social_network_link_repeater_ItemCreated(object sender, RepeaterItemEventArgs e) {
			if ((e.Item.ItemType == ListItemType.Item) || (e.Item.ItemType == ListItemType.AlternatingItem)) {
				DbDataRecord currentRecord = e.Item.DataItem as DbDataRecord;
				if (currentRecord != null) {
					_socialNetworkId = Utils.getDbIntegerValue(currentRecord[(int)SocialNetworkLinkReaderFields.SocialNetworkId], 0);

					_socialNetworkName = Utils.getDbStringValue(currentRecord[(int)SocialNetworkLinkReaderFields.Name], string.Empty);

					_socialNetworkUrl = Utils.getDbStringValue(currentRecord[(int)SocialNetworkLinkReaderFields.NetworkURL], string.Empty);
					_socialNetworkIcon = Utils.getDbStringValue(currentRecord[(int)SocialNetworkLinkReaderFields.Icon], string.Empty);

					if (_socialNetworkName == "LinkedIn" & !IsPostBack) { //get Linkedin Group Id for request
						Uri url = new Uri(_socialNetworkUrl);

						_linkedinGroupId = HttpUtility.ParseQueryString(url.Query).Get("gid");
					}
					if (_socialNetworkName == "FaceBook" & !IsPostBack) { //get Facebook Page for request
						
						Uri uri = new Uri(_socialNetworkUrl);
						string cleanUrl = uri.GetLeftPart(UriPartial.Path); //removes any query string user might have saved in their facebook settings
						cleanUrl = cleanUrl.TrimEnd(new[] { '/' });
						int LastIndex = cleanUrl.LastIndexOf("/");
						_facebookId = cleanUrl.Substring(LastIndex+1,(cleanUrl.Length - LastIndex -1));

						if (_socialNetworkUrl.Contains("groups")) {
							_isFacebookGroup = "true";
						}
						else {
							_isFacebookGroup = "false";
						}

					}
				}
			}
		}

		// check_box_Click
		protected void check_box_Click(Object sender, EventArgs e) {
			_networkClicked = true;
			determineSelection();

			requestTokens();
			//getFilterItems();
		}

		//determineSelection
		protected void determineSelection() {
			foreach (RepeaterItem rpItem in social_network_repeater.Items) {
				CheckBox chkbx = rpItem.FindControl("social_check") as CheckBox;
				if (chkbx.Checked) {
					string chosen = chkbx.Attributes["socialId"].ToString();
					switch (chosen) {
						case "1":
							_linkedInChecked = true;
							break;
						case "2":
							_twitterChecked = true;
							break;
						case "3":
							_faceBookChecked = true;
							break;
					}
				}
			}
		}

		//clearTokens
		protected void clearTokens(Object sender, EventArgs e) {
			_tokenFilter.clearTokens();
			_tokenFilter.clearCacheKeys();
			foreach (RepeaterItem item in social_network_repeater.Items) {
				CheckBox cb = (CheckBox)item.FindControl("social_check");
				if (cb != null) {
					cb.Checked = false;
				}
			}
			requestTokens();
		}
		
		//requestTokens
		protected void requestTokens() {
			//if token does not exist in filter, redirect and get token
			if (_twitterChecked) {
				if (_tokenFilter.twitterAuthToken == Constants.InvalidIdString || string.IsNullOrEmpty(_tokenFilter.twitterAuthToken) || _tokenFilter.twitterAuthToken == null) { //if token does not exist in filter, redirect and get token
					_twitterCacheKey = _twitterCacheKeyPrefix + "-" + _requestNonce + "-" + club.clubIdString; //creates random clubid cache key
					_twitterSecretKey = _twitterSecretKeyPrefix + "-" + _requestNonce + "-" + club.clubIdString; //uses same nonce in token cache key to create a token secret key
					oTwitter.CallBackUrl = "http://www.clubexpress.com/basic_modules/club_admin/website/auth_callback.aspx?type=twitter&state=" + _twitterCacheKey;
					_tokenFilter.twitterCacheKey = _twitterCacheKey;
					_tokenFilter.twitterSecretKey = _twitterSecretKey;
					_twitterString = oTwitter.AuthorizationLinkGet();
				//	Response.Write("<script>window.open('" + _twitterString + "', \"_blank\", \"toolbar=yes, scrollbars=yes, resizable=yes, width=580, height=400\");</script>");
					_popupString = _twitterString;
				}

			}
			if (_linkedInChecked) {
				if (_tokenFilter.linkedinToken == Constants.InvalidIdString || string.IsNullOrEmpty(_tokenFilter.linkedinToken) || _tokenFilter.linkedinToken == null) {
					_linkedInCacheKey = _linkedInCacheKeyPrefix + "-" + _requestNonce + "-" + club.clubIdString; //creates random clubid cache key
					_tokenFilter.linkedInCacheKey = _linkedInCacheKey;
					_linkedInString = oLinkedIn.AuthorizationLinkGet(_linkedInCacheKey);
				//	Response.Write("<script>window.open('" + _linkedInString + "', \"_blank\", \"toolbar=yes, scrollbars=yes, resizable=yes, width=580, height=400\");</script>");
					_popupString = _linkedInString;
				}
			}
			if (_faceBookChecked) {
				if (_tokenFilter.facebookToken == Constants.InvalidIdString || string.IsNullOrEmpty(_tokenFilter.facebookToken) || _tokenFilter.facebookToken ==null) { //if token does not exist in filter, redirect and get token
					_facebookCacheKey = _facebookCacheKeyPrefix + "-" + _requestNonce + "-" + club.clubIdString; //creates random clubid cache key
					_tokenFilter.facebookCacheKey = _facebookCacheKey;
					_facebookString = oFB.AuthorizationLinkGet(_facebookCacheKey);
				//	Response.Write("<script>window.open('" + _facebookString + "', \"_blank\", \"toolbar=yes, scrollbars=yes, resizable=yes, width=580, height=450\");</script>");
					_popupString = _facebookString;
				}
			}
			if (!string.IsNullOrEmpty(_popupString)) {
				Response.Write("<script>window.open('" + _popupString + "', \"_blank\", \"toolbar=yes, scrollbars=yes, resizable=yes, width=580, height=450\");</script>");
			}
			sessionData.addDataItem("TokenFilter", _tokenFilter);
		}
		protected void getFilterItems() {
			_tokenFilter = sessionData.getDataItem("TokenFilter") as TokenFilter;
			if (_tokenFilter == null) { 
				_tokenFilter = new TokenFilter();
				sessionData.addDataItem("TokenFilter", _tokenFilter);
			}
			_linkedInCacheKey = _tokenFilter.linkedInCacheKey;
			_facebookCacheKey = _tokenFilter.facebookCacheKey;
			_twitterCacheKey = _tokenFilter.twitterCacheKey;
			if (_twitterCacheKey != Constants.InvalidIdString && (_tokenFilter.twitterAuthToken == Constants.InvalidIdString || _tokenFilter.twitterAuthToken == null)) {
				_tokenFilter.twitterAuthToken = CacheManager.getItemFromCache(_tokenFilter.twitterCacheKey, club.clubIdString) as string;
				_tokenFilter.twitterSecret = CacheManager.getItemFromCache(_tokenFilter.twitterSecretKey, club.clubIdString) as string;
			}
			if (_linkedInCacheKey != Constants.InvalidIdString && (_tokenFilter.linkedinToken == Constants.InvalidIdString || _tokenFilter.linkedinToken == null)) {
				_tokenFilter.linkedinToken = CacheManager.getItemFromCache(_tokenFilter.linkedInCacheKey, club.clubIdString) as string;
			}
			if (_facebookCacheKey != Constants.InvalidIdString && (_tokenFilter.facebookToken == Constants.InvalidIdString || _tokenFilter.facebookToken == null)) {
				_tokenFilter.facebookToken = CacheManager.getItemFromCache(_tokenFilter.facebookCacheKey, club.clubIdString) as string;
			}
			_twitterToken = _tokenFilter.twitterAuthToken;
			_linkedInToken = _tokenFilter.linkedinToken;
			_facebookToken = _tokenFilter.facebookToken;
		}
		//getPageToken()

		protected void getPageToken() {
			if (_tokenFilter.facebookPageToken == null || _tokenFilter.facebookPageToken == Constants.InvalidIdString) {

				//get the page id so we can query the me/accounts data for the correct access token
				string pageIdUrl = "https://graph.facebook.com/" + _facebookId + "?access_token=" +_tokenFilter.facebookToken;
				string pageIdResponse = oAuthFacebook.PageTokenRequest(pageIdUrl, String.Empty);
				//gets facebook numerical page ID for facebook page stored in Social Networking Options
				if (pageIdResponse.Length > 0) {
					JToken token = JObject.Parse(pageIdResponse);
					string pageid = (string)token.SelectToken("id");
					//Store the returned access_token
					if (pageid != null) {
						_tokenFilter.facebookPageId = pageid;
					}
				}

				//get Json Object for me/accounts data -> Get page id token
				string userAccountUrl = "https://graph.facebook.com/me/accounts?access_token=" + _tokenFilter.facebookToken;
				//Request for Facebook Json Object
				string response = oAuthFacebook.PageTokenRequest(userAccountUrl, String.Empty);
				//Deserialize Json Object
				if (response.Length > 0) {
				RootObject JsonObject = JsonConvert.DeserializeObject<RootObject>(response);
				//Iterate through data objects in Json to get page Id token for posting
				//Page ID in json data must match numerical page Id requested above
					foreach (FbData dataItem in JsonObject.data) {
						if (dataItem.id == _tokenFilter.facebookPageId) {
							_tokenFilter.facebookPageToken = dataItem.access_token;
						}
					}
				}

			}
		}
		// save_button_Click
		protected void save_button_Click(Object sender, EventArgs e) {

			_linkedinGroupId = gidHiddenField.Value;
			_facebookId = fbHiddenField.Value;
			_isFacebookGroup = fbGroupField.Value;
			getFilterItems(); //Items should be in system cache by now, go get them.
			_tokenFilter.clearCacheKeys(); //clear the Cache Keys used to retrieve tokens from system cache. They'll be of no further use and expire anyway.
			_shareText = social_text.Text;
			determineSelection();
			//do facebook post if item is checked
			if (_faceBookChecked || _linkedInChecked || _twitterChecked) {
				makeApiCall();
				baseContentPage.returnToPreviousPage(true, _postType + " Shared", false);
			}
			else {
				Response.Write("<script>alert('You must select a network before sharing');</script>");
				
			}
		}
		protected void makeApiCall() {
			if (_faceBookChecked) {
				StringBuilder fbPost = new StringBuilder();
				fbPost.Append("message=" + HttpUtility.UrlEncode(_shareText));
				fbPost.Append("&caption=" + HttpUtility.UrlEncode(_postTitle));
				fbPost.Append("&link=" + HttpUtility.UrlEncode(_formattedURL));
				fbPost.Append("&description=" + HttpUtility.UrlEncode(_shareText));
				fbPost.Append("&name=" + HttpUtility.UrlEncode(_postTitle));
				fbPost.Append("&scrape=true");
				string fbFormattedPost = fbPost.ToString();
				//make api call
				if (_isFacebookGroup == "false") {
					getPageToken();
				}
				if (_tokenFilter.facebookPageToken != null && _tokenFilter.facebookPageToken != Constants.InvalidIdString) {
					var url = "https://graph.facebook.com/" + _facebookId + "/feed?access_token=" + _tokenFilter.facebookPageToken;
					var json = oFB.WebRequest(oAuthFacebook.Method.POST, url, fbFormattedPost);
				}
				else if (_tokenFilter.facebookToken != null && _tokenFilter.facebookToken != Constants.InvalidIdString) { //no page token received
					if (_isFacebookGroup == "true") { //no page token received if its a group, post to group
						var url = "https://graph.facebook.com/"+ _facebookId + "/feed?access_token=" + _tokenFilter.facebookToken;
						var json = oFB.WebRequest(oAuthFacebook.Method.POST, url, fbFormattedPost);
					}
					else if (_isFacebookGroup == "false" && (_tokenFilter.facebookPageToken == null || _tokenFilter.facebookPageToken == Constants.InvalidIdString)) { //fallback and post on users personal facebook page
						var url = "https://graph.facebook.com/me/feed?access_token=" + _tokenFilter.facebookToken;
						var json = oFB.WebRequest(oAuthFacebook.Method.POST, url, fbFormattedPost);
					}
					else {
						var url = "https://graph.facebook.com/me/feed?access_token=" + _tokenFilter.facebookToken;
						var json = oFB.WebRequest(oAuthFacebook.Method.POST, url, fbFormattedPost);
					}
				}
				else {
					Response.Write("<script>alert('Facebook Authorization Error. Please Authorize Facebook');</script>");
				}
				
			}
			
			if (_linkedInChecked) {
				//posts to Linkedin via XML. There is a json alternative, but XML seems more stable
				string encodedTitle = SecurityElement.Escape(_postTitle); //escapes <>'"
				string encodedText = SecurityElement.Escape(_shareText); //escapes <>'"
				StringBuilder xmlpost = new StringBuilder();
				xmlpost.Append("<?xml version='1.0' encoding='UTF-8'?><post><title>" + encodedTitle + "</title>");
				xmlpost.Append("<summary>" + encodedText + "</summary><content>");
				xmlpost.Append("<submitted-url>" + HttpUtility.HtmlEncode(_formattedURL) + "</submitted-url><submitted-image-url>" + string.Empty + "</submitted-image-url>");
				xmlpost.Append("<title>" + string.Empty + "</title><description>" + _postType + "</description></content></post>");
				string formattedLpost = xmlpost.ToString();
				if (_tokenFilter.linkedinToken != null) {
					var requestUrl = "https://api.linkedin.com/v1/groups/" + _linkedinGroupId + "/posts?oauth2_access_token=" + _tokenFilter.linkedinToken;
					//make api call
					var xmlreturn = oLinkedIn.WebRequest(oAuthLinkedIn.Method.POST, requestUrl, formattedLpost);
				}
				else {
					Response.Write("<script>alert('Linkedin Authorization Error. Please Authorize Linkedin');</script>");
				}


			}
			if (_twitterChecked) {
				if (_tokenFilter.twitterAuthToken != null || _tokenFilter.twitterAuthToken != Constants.InvalidIdString) {
					oTwitter.TokenSecret = _tokenFilter.twitterSecret;
					oTwitter.Token = _tokenFilter.twitterAuthToken;
					//make api call
					oTwitter.PostMessageToTwitter(_shareText + "  " + _formattedURL);
				}
				else {
					Response.Write("<script>alert('Twitter Authorization Error. Please Authorize Twitter');</script>");
				}
			}
		}
		// pageName property
		public override string pageName {
			get { return _pageName; }
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e) {
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
