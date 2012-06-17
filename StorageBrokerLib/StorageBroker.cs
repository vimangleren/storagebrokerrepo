using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WarmPenguin.StorageBroker
{
	internal static class DataFeed
	{
		const string baseServiceUrl = "http://localhost/penguindata.svc/"; //"http://winelli.azurewebsites.net/penguindata.svc/"; 

		public static XNamespace xa = "http://www.w3.org/2005/Atom";
		public static XNamespace xd = "http://schemas.microsoft.com/ado/2007/08/dataservices";
		public static XNamespace xm = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

		public static async Task<string> GetStringAsync(string query)
		{
			var client = new HttpClient();
			client.MaxResponseContentBufferSize = 32 * 1024 * 1024;

			try
			{
				var response = await client.GetAsync(baseServiceUrl + query);
				return await response.Content.ReadAsStringAsync();
			}
			catch (WebException e)
			{ 
				
			}

			return null;
		}
	}

	public class DealImageData
	{
		public static async Task<DealImage> GetDealImageAsync(string dealId, string size)
		{
			if (string.IsNullOrEmpty(size))
				size = "original";

			var data = await DataFeed.GetStringAsync("DealImages?$filter=(DealRowKey eq '" + dealId + "') and (Handled eq true) and (ImageSize eq '" + size + "')");

			var dealImages =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new DealImage
				{
					Name = (string)properties.Element(DataFeed.xd + "Name"),
					ImageUrl = (string)properties.Element(DataFeed.xd + "ImageUrl")
				};

			return dealImages.First();
		}

		public static async Task<DealImage[]> GetDealImagesAsync(string dealId, string size)
		{
			if (string.IsNullOrEmpty(size))
				size = "original";

			var data = await DataFeed.GetStringAsync("DealImages?$filter=(DealRowKey eq '" + dealId + "') and (Handled eq true) and (ImageSize eq '" + size + "')");

			var dealImages =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new DealImage
				{
					Name = (string)properties.Element(DataFeed.xd + "Name"),
					ImageUrl = (string)properties.Element(DataFeed.xd + "ImageUrl")
				};

			return dealImages.ToArray();
		}
	}

	public class CountryData
	{
		const string allCountriesQuery = "Countries";

		public static async Task<Country> GetCountryAsync(string countryId)
		{
			var data = await DataFeed.GetStringAsync("Countries?$filter=RowKey eq '" + countryId + "'");
			var countries =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new Country
				{
					CountryId = (string)properties.Element(DataFeed.xd + "RowKey"),
					CountryName = (string)properties.Element(DataFeed.xd + "CountryName"),
					CountryFlag250x250Url = (string)properties.Element(DataFeed.xd + "CountryFlag250x250Url")
				};
			return countries.First();
		}

		public static async Task<Country[]> GetCountriesAsync()
		{
			var data = await DataFeed.GetStringAsync(allCountriesQuery);
			var countries =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new Country
				{
					CountryId = (string)properties.Element(DataFeed.xd + "RowKey"),
					CountryName = (string)properties.Element(DataFeed.xd + "CountryName"),
					CountryFlag250x250Url = (string)properties.Element(DataFeed.xd + "CountryFlag250x250Url")
				};
			return countries.ToArray();
		}
	}

	public class DealData
	{
		const string allDealsQuery = "Deals";

		public static async Task<Deal[]> GetDealsAsync()
		{
			var data = await DataFeed.GetStringAsync(allDealsQuery);
			var deals =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new Deal
				{
					Title = (string)entry.Element(DataFeed.xa + "title"),
					Description = (string)properties.Element(DataFeed.xd + "description")
				};
			return deals.ToArray();
		}

		public static async Task<Deal> GetDealAsync(string dealId)
		{
			var data = await DataFeed.GetStringAsync("Deals?$filter=RowKey eq '" + dealId + "'");

			var deals =
					from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
					let properties = entry.Element(DataFeed.xm + "properties")
					where (properties.Element(DataFeed.xd + "RowKey").Value == dealId) && (Convert.ToBoolean(properties.Element(DataFeed.xd + "Active").Value) == true)
					select new Deal
					{
						DealId = (string)properties.Element(DataFeed.xd + "RowKey"),
						Title = (string)properties.Element(DataFeed.xd + "Title"),
						Description = (string)properties.Element(DataFeed.xd + "Description"),
						BeginDeal = (DateTime)properties.Element(DataFeed.xd + "BeginDeal"),
						EndDeal = (DateTime)properties.Element(DataFeed.xd + "EndDeal"),
						SavingsPercentage = (int)properties.Element(DataFeed.xd + "SavingsPercentage"),
						StockAmount = (int)properties.Element(DataFeed.xd + "StockAmount"),
						Price = (double)properties.Element(DataFeed.xd + "Price"),
						CountryRowKey = (string)properties.Element(DataFeed.xd + "CountryRowKey"),
						Redwine = (bool)properties.Element(DataFeed.xd + "Redwine"),
						White = (bool)properties.Element(DataFeed.xd + "WhiteWine"),
						CategoryRowKey = (string)properties.Element(DataFeed.xd + "CategoryRowKey")
					};
			return deals.First();
		}


		public static async Task<Deal[]> GetDealsByCountryAsync(string countryId)
		{
			var data = await DataFeed.GetStringAsync("Deals?$filter=CountryRowKey eq '" + countryId + "'");

			var deals =
					from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
					let properties = entry.Element(DataFeed.xm + "properties")
					where (Convert.ToBoolean(properties.Element(DataFeed.xd + "Active").Value) == true)
					select new Deal
					{
						DealId = (string)properties.Element(DataFeed.xd + "RowKey"),
						Title = (string)properties.Element(DataFeed.xd + "Title"),
						Description = (string)properties.Element(DataFeed.xd + "Description"),
						BeginDeal = (DateTime)properties.Element(DataFeed.xd + "BeginDeal"),
						EndDeal = (DateTime)properties.Element(DataFeed.xd + "EndDeal"),
						SavingsPercentage = (int)properties.Element(DataFeed.xd + "SavingsPercentage"),
						StockAmount = (int)properties.Element(DataFeed.xd + "StockAmount"),
						Price = (double)properties.Element(DataFeed.xd + "Price"),
						CountryRowKey = (string)properties.Element(DataFeed.xd + "CountryRowKey"),
						Redwine = (bool)properties.Element(DataFeed.xd + "Redwine"),
						White = (bool)properties.Element(DataFeed.xd + "WhiteWine"),
						CategoryRowKey = (string)properties.Element(DataFeed.xd + "CategoryRowKey")
					};
			return deals.ToArray();
		}

		public static async Task<Deal[]> GetDealByWineTypeAsync(bool redWine)
		{
			var data = string.Empty;
			if (redWine)
				data = await DataFeed.GetStringAsync("Deals?$filter=Redwine eq true");
			else
				data = await DataFeed.GetStringAsync("Deals?$filter=WhiteWine eq false");

			var deals =
					from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
					let properties = entry.Element(DataFeed.xm + "properties")
					where (Convert.ToBoolean(properties.Element(DataFeed.xd + "Active").Value) == true)
					select new Deal
					{
						DealId = (string)properties.Element(DataFeed.xd + "RowKey"),
						Title = (string)properties.Element(DataFeed.xd + "Title"),
						Description = (string)properties.Element(DataFeed.xd + "Description"),
						BeginDeal = (DateTime)properties.Element(DataFeed.xd + "BeginDeal"),
						EndDeal = (DateTime)properties.Element(DataFeed.xd + "EndDeal"),
						SavingsPercentage = (int)properties.Element(DataFeed.xd + "SavingsPercentage"),
						StockAmount = (int)properties.Element(DataFeed.xd + "StockAmount"),
						Price = (double)properties.Element(DataFeed.xd + "Price"),
						CountryRowKey = (string)properties.Element(DataFeed.xd + "CountryRowKey"),
						Redwine = (bool)properties.Element(DataFeed.xd + "Redwine"),
						White = (bool)properties.Element(DataFeed.xd + "WhiteWine"),
						CategoryRowKey = (string)properties.Element(DataFeed.xd + "CategoryRowKey")
					};
			return deals.Take(20).ToArray();
		}


		public static async Task<Deal[]> GetDealsAsync(string countryId, string categoryId)
		{
			var data = await DataFeed.GetStringAsync(allDealsQuery);

			var deals =
					from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
					let properties = entry.Element(DataFeed.xm + "properties")
					where (properties.Element(DataFeed.xd + "CategoryRowKey").Value == categoryId) && (properties.Element(DataFeed.xd + "CountryRowKey").Value == countryId) && (Convert.ToBoolean(properties.Element(DataFeed.xd + "Active").Value) == true)
					select new Deal
					{
						DealId = (string)properties.Element(DataFeed.xd + "RowKey"),
						Title = (string)properties.Element(DataFeed.xd + "Title"),
						Description = (string)properties.Element(DataFeed.xd + "Description"),
						BeginDeal = (DateTime)properties.Element(DataFeed.xd + "BeginDeal"),
						EndDeal = (DateTime)properties.Element(DataFeed.xd + "EndDeal"),
						SavingsPercentage = (int)properties.Element(DataFeed.xd + "SavingsPercentage"),
						StockAmount = (int)properties.Element(DataFeed.xd + "StockAmount"),
						Price = (double)properties.Element(DataFeed.xd + "Price"),
						CountryRowKey = (string)properties.Element(DataFeed.xd + "CountryRowKey"),
						Redwine = (bool)properties.Element(DataFeed.xd + "Redwine"),
						White = (bool)properties.Element(DataFeed.xd + "WhiteWine"),
						CategoryRowKey = (string)properties.Element(DataFeed.xd + "CategoryRowKey")
					};
			return deals.ToArray();
		}


		public static async Task<Deal[]> GetDealsAsync(string categoryId)
		{
			var data = await DataFeed.GetStringAsync(allDealsQuery);

			var deals =
					from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
					let properties = entry.Element(DataFeed.xm + "properties")
					where (properties.Element(DataFeed.xd + "CategoryRowKey").Value == categoryId) && (Convert.ToBoolean(properties.Element(DataFeed.xd + "Active").Value) == true)
					select new Deal
					{
						DealId = (string)properties.Element(DataFeed.xd + "RowKey"),
						Title = (string)properties.Element(DataFeed.xd + "Title"),
						Description = (string)properties.Element(DataFeed.xd + "Description"),
						BeginDeal = (DateTime)properties.Element(DataFeed.xd + "BeginDeal"),
						EndDeal = (DateTime)properties.Element(DataFeed.xd + "EndDeal"),
						SavingsPercentage = (int)properties.Element(DataFeed.xd + "SavingsPercentage"),
						StockAmount = (int)properties.Element(DataFeed.xd + "StockAmount"),
						Price = (double)properties.Element(DataFeed.xd + "Price"),
						CountryRowKey = (string)properties.Element(DataFeed.xd + "CountryRowKey"),
						Redwine = (bool)properties.Element(DataFeed.xd + "Redwine"),
						White = (bool)properties.Element(DataFeed.xd + "WhiteWine"),
						CategoryRowKey = (string)properties.Element(DataFeed.xd + "CategoryRowKey")
					};
			return deals.ToArray();
		}

		public static async Task<int> GetDealCount(string countryId, string categoryId)
		{
			var data = await DataFeed.GetStringAsync("Deals?$filter=CountryRowKey eq '" + countryId + "'");

			var deals =
					(from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
					 let properties = entry.Element(DataFeed.xm + "properties")
					 where (properties.Element(DataFeed.xd + "CountryRowKey").Value == countryId) && (properties.Element(DataFeed.xd + "CategoryRowKey").Value == categoryId)
					 && (Convert.ToBoolean(properties.Element(DataFeed.xd + "Active").Value) == true)
					 select entry).Count();

			return deals;
		}

		public static async Task<int> GetDealCount(string categoryId)
		{
			var data = await DataFeed.GetStringAsync("Deals?$filter=CategoryRowKey eq '" + categoryId + "'");

			var deals =
					(from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
					 let properties = entry.Element(DataFeed.xm + "properties")
					 where (properties.Element(DataFeed.xd + "CategoryRowKey").Value == categoryId)
					 && (Convert.ToBoolean(properties.Element(DataFeed.xd + "Active").Value) == true)
					 select entry).Count();

			return deals;
		}
	}

	public class CategoriesData
	{
		const string allCategoriesQuery = "Categories";

		public static async Task<Category> GetCategoryAsync(string categoryId)
		{
			var data = await DataFeed.GetStringAsync("Categories?$filter=RowKey eq '" + categoryId + "'");

			var categories =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new Category
				{
					CategoryId = (string)properties.Element(DataFeed.xd + "RowKey"),
					Name = (string)properties.Element(DataFeed.xd + "Name"),
					LatestDealId = (string)properties.Element(DataFeed.xd + "LastestDealRowKey")
				};
			return categories.First();
		}

		public static async Task<Category[]> GetCategoriesAsync()
		{
			var data = await DataFeed.GetStringAsync(allCategoriesQuery);

			var categories =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new Category
				{
					CategoryId = (string)properties.Element(DataFeed.xd + "RowKey"),
					Name = (string)properties.Element(DataFeed.xd + "Name"),
					LatestDealId = (string)properties.Element(DataFeed.xd + "LastestDealRowKey")
				};
			return categories.ToArray();
		}
	}

	public class UsersData
	{
		public static async Task<bool> CreateUser(User user)
		{
			var data = await DataFeed.GetStringAsync("CreateUser?email='" + user.Email + "'&name='" + user.ScreenName + "'&address='" + user.Address + "'&password='" + user.Password + "'");
			var success = XDocument.Parse(data).Root.Value;

			return Convert.ToBoolean(success);
		}

		public static async Task<bool> UserExists(string email)
		{
			var data = await DataFeed.GetStringAsync("Users?$filter=Email eq '" + email + "'");

			var users =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new
				{
					Email = properties.Element(DataFeed.xd + "Email").Value
				};
			try
			{
				var user = users.FirstOrDefault();

				if (user == null)
				{
					return false;
				}
			}
			catch (Exception e)
			{
				//log e
			}

			return true;
		}

		public static async Task<bool> LoginUser(string email, string password)
		{
			var data = await DataFeed.GetStringAsync("/Users?$filter=Email eq '" + email + "'&password='" + password + "'");

			var user =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new
				{
					Email = properties.Element(DataFeed.xd + "Email").Value
				};
			try
			{
				var u = user.FirstOrDefault();

				if (u == null)
				{
					return false;
				}
			}
			catch (Exception e)
			{

			}

			return true;
		}

		public static async Task<User> GetByToken(string token)
		{
			var data = await DataFeed.GetStringAsync("/Users?$filter=RowKey eq '" + token + "'");

			var user =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new User
				{
					Address = properties.Element(DataFeed.xd + "Address").Value,
					ScreenName = properties.Element(DataFeed.xd + "ScreenName").Value,
					Email = properties.Element(DataFeed.xd + "Email").Value
				};
			try
			{
				var u = user.FirstOrDefault();

				return u;
			}
			catch (Exception e)
			{

			}

			return null;
		}


		public static async Task<User> GetByEmail(string email)
		{
			var data = await DataFeed.GetStringAsync("/Users?$filter=Email eq '" + email + "'");

			var user =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new User
				{
					Address = properties.Element(DataFeed.xd + "Address").Value,
					ScreenName = properties.Element(DataFeed.xd + "ScreenName").Value,
					Email = properties.Element(DataFeed.xd + "Email").Value
				};
			try
			{
				var u = user.FirstOrDefault();

				return u;
			}
			catch (Exception e)
			{

			}

			return null;
		}

	}


	public class OrdersData
	{
		const string method = "InitOrder";

		public static async void InitOrder(string orderId, string userId)
		{
			var data = await DataFeed.GetStringAsync(method + "?orderId='" + orderId + "'&userId='" + userId + "'");
		}

		public static async Task<Order> CheckOrder(string orderId)
		{
			var data = await DataFeed.GetStringAsync("/Orders?$filter=RowKey eq '" + orderId + "'");

			var order =
				from entry in XDocument.Parse(data).Descendants(DataFeed.xa + "entry").Descendants(DataFeed.xa + "content")
				let properties = entry.Element(DataFeed.xm + "properties")
				select new Order
				{
					Handled = (bool)properties.Element(DataFeed.xd + "Handled")
				};

			return order.First();
		}
	}


	public class Account
	{
		public Account()
		{

		}

		public string ScreenName { get; set; }
		public string Email { get; set; }
	}

	/// <summary>
	/// A user is a person that buys products
	/// </summary>
	public class User
	{
		public User()
		{

		}

		public string ScreenName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Address { get; set; }
	}

	/// <summary>
	/// 
	/// </summary>
	public class Deal
	{
		public Deal()
		{
		}


		private string _dealId = string.Empty;
		public string DealId
		{
			get
			{
				return _dealId;
			}
			set
			{
				if (_dealId != value)
				{
					_dealId = value;

				}
			}
		}

		public string Title { get; set; }
		public string Description { get; set; }
		public double Price { get; set; }
		public DateTime BeginDeal { get; set; }
		public DateTime EndDeal { get; set; }
		public int SavingsPercentage { get; set; }
		public string AccountRowKey { get; set; }
		public int StockAmount { get; set; }
		public string CategoryRowKey { get; set; }
		public bool Active { get; set; }

		//winelli specific
		public string CountryRowKey { get; set; }
		public bool Redwine { get; set; }
		public bool White { get; set; }
	}

	public class Country
	{
		public Country()
		{

		}

		public string CountryId { get; set; }
		public string CountryName { get; set; }
		public string CountryFlag250x250Url { get; set; }
	}


	public class DealImage
	{
		public DealImage()
		{

		}

		public string DealRowKey { get; set; }
		public string ParentImage { get; set; }
		public string ImageUrl { get; set; }
		public string ImageSize { get; set; }
		public bool Handled { get; set; }
		public string Name { get; set; }
	}

	public class Order
	{
		public Order()
		{

		}

		public string UserRowKey { get; set; }
		public string OrderRowKey { get; set; }
		public DateTime OrderedDate { get; set; }
		public bool Handled { get; set; }
		public bool Success { get; set; }
	}

	public class Category : INotifyPropertyChanged
	{
		public Category()
		{

		}

		private string _categoryId = string.Empty;

		public string CategoryId
		{
			get
			{
				return this._categoryId;
			}
			set
			{
				if (value != this._categoryId)
				{
					this._categoryId = value;
					NotifyPropertyChanged("CategoryId");
				}
			}
		}

		private string _name = string.Empty;

		public string Name
		{
			set
			{
				if (value != this._categoryId)
				{
					this._name = value;
					NotifyPropertyChanged("Name");
				}
			}
			get
			{
				return _name;
			}
		}

		private string _latestDealId = string.Empty;

		public string LatestDealId
		{
			get
			{
				return _latestDealId;
			}
			set
			{
				if (value != this._latestDealId)
				{
					this._latestDealId = value;
					NotifyPropertyChanged("LatestDealId");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

	}
}
