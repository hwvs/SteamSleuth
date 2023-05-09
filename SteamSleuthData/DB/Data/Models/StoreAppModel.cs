// Ignore annoying warnings for unused fields, CS8168
// I don't get to set the rules around what's used and not
// - that's on Valve.
#nullable enable
#pragma warning disable CS8618


using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace SteamSleuthData.Data.Models;
public class StoreAppModel
	{
		static StoreAppModel()
		{
			// TODO: This is a hack to force it to be valid C# at program startup
			BsonClassMap.RegisterClassMap<StoreAppModel>(cm =>
			{
				cm.AutoMap();
				cm.MapIdMember(c => c._id);
				cm.MapMember(c => c.LastUpdated).SetElementName("_last_updated");
			});
		}
		
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonElement("_id")]
		public ObjectId _id { get; set; }

		[BsonRepresentation(BsonType.DateTime)] [BsonElement("_last_updated")]
		public DateTime LastUpdated = DateTime.MinValue;

		[BsonElement("type")]
		public string Type { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("steam_appid")]
		public int SteamAppId { get; set; }

		[BsonElement("required_age")]
		public int RequiredAge { get; set; }

		[BsonElement("is_free")]
		public bool IsFree { get; set; }

		[BsonElement("detailed_description")]
		public string DetailedDescription { get; set; }

		[BsonElement("about_the_game")]
		public string AboutTheGame { get; set; }

		[BsonElement("short_description")]
		public string ShortDescription { get; set; }

		[BsonElement("supported_languages")]
		public string SupportedLanguages { get; set; }

		[BsonElement("header_image")]
		public string HeaderImage { get; set; }

		[BsonElement("website")]
		public string Website { get; set; }

		[BsonElement("pc_requirements")]
		public object PcRequirements { get; set; }

		[BsonElement("mac_requirements")]
		public object MacRequirements { get; set; }

		[BsonElement("linux_requirements")]
		public object LinuxRequirements { get; set; }

		[BsonElement("developers")]
		public string[] Developers { get; set; }

		[BsonElement("publishers")]
		public string[] Publishers { get; set; }

		[BsonElement("price_overview")]
		public StoreAppPriceOverview PriceOverview { get; set; }

		[BsonElement("packages")]
		public int[] Packages { get; set; }

		[BsonElement("package_groups")]
		public StoreAppPackageGroup[] PackageGroups { get; set; }

		[BsonElement("platforms")]
		public StoreAppPlatforms Platforms { get; set; }

		[BsonElement("categories")]
		public StoreAppIdDescriptionPair[] Categories { get; set; }

		[BsonElement("genres")]
		public StoreAppIdDescriptionPair[] Genres { get; set; }

		[BsonElement("screenshots")]
		public StoreAppScreenshot[] Screenshots { get; set; }

		[BsonElement("movies")]
		public StoreAppMovie[] Movies { get; set; }
		
		[BsonElement("release_date")]
		public StoreAppReleaseDate ReleaseDate { get; set; }

		[BsonElement("support_info")]
		public StoreAppSupportInfo SupportInfo { get; set; }

		[BsonElement("background")]
		public string Background { get; set; }

		[BsonElement("background_raw")]
		public string BackgroundRaw { get; set; }

		[BsonElement("content_descriptors")]
		public StoreAppContentDescriptors ContentDescriptors { get; set; }
		
		// Basic ToString
		public override string ToString()
		{
			return $"StoreAppModel: {Name} ({SteamAppId})";
		}
		
	}

public class StoreAppPriceOverview
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonElement("_id")]
		public ObjectId _id { get; set; }
		
		[BsonElement("currency")]
		public string Currency { get; set; }

		[BsonElement("initial")]
		public int Initial { get; set; }

		[BsonElement("final")]
		public int Final { get; set; }

		[BsonElement("discount_percent")]
		public int DiscountPercent { get; set; }

		[BsonElement("initial_formatted")]
		public string InitialFormatted { get; set; }

		[BsonElement("final_formatted")]
		public string FinalFormatted { get; set; }
	}

public class StoreAppPackageGroup
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonElement("_id")]
		public ObjectId _id { get; set; }
		
		
		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("title")]
		public string Title { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }

		[BsonElement("selection_text")]
		public string SelectionText { get; set; }

		[BsonElement("save_text")]
		public string SaveText { get; set; }

		[BsonElement("display_type")]
		public int DisplayType { get; set; }

		[BsonElement("is_recurring_subscription")]
		public string IsRecurringSubscription { get; set; }

		[BsonElement("subs")]
		public StoreAppSub[] Subs { get; set; }
	}

public class StoreAppSub
	{
		
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonElement("_id")]
		public ObjectId _id { get; set; }
		
		[BsonElement("packageid")]
		public int PackageId { get; set; }

		[BsonElement("percent_savings_text")]
		public string PercentSavingsText { get; set; }

		[BsonElement("percent_savings")]
		public int PercentSavings { get; set; }

		[BsonElement("option_text")]
		public string OptionText { get; set; }

		[BsonElement("option_description")]
		public string OptionDescription { get; set; }

		[BsonElement("can_get_free_license")]
		public string CanGetFreeLicense { get; set; }

		[BsonElement("is_free_license")]
		public bool IsFreeLicense { get; set; }

		[BsonElement("price_in_cents_with_discount")]
		public int PriceInCentsWithDiscount { get; set; }
	}

public class StoreAppPlatforms
	{
		
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonElement("_id")]
		public ObjectId _id { get; set; }
		
		[BsonElement("windows")]
		public bool Windows { get; set; }

		[BsonElement("mac")]
		public bool Mac { get; set; }

		[BsonElement("linux")]
		public bool Linux { get; set; }
	}

public class StoreAppIdDescriptionPair
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonElement("_id")]
		public ObjectId _id { get; set; }
		
		[BsonElement("id")]
		public int Id { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }
	}

public class StoreAppScreenshot
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonElement("_id")]
		public ObjectId _id { get; set; }
		
		[BsonElement("id")]
		public int Id { get; set; }

		[BsonElement("path_thumbnail")]
		public string PathThumbnail { get; set; }

		[BsonElement("path_full")]
		public string PathFull { get; set; }
	}

public class StoreAppMovie
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		[BsonElement("_id")]
		public ObjectId _id { get; set; }
		
		[BsonElement("id")]
		public int Id { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("thumbnail")]
		public string Thumbnail { get; set; }

		[BsonElement("webm")]
		public StoreAppMovieWebm Webm { get; set; }

		[BsonElement("mp4")]
		public StoreAppMovieMp4 Mp4 { get; set; }

		[BsonElement("highlight")]
		public bool Highlight { get; set; }
	}

public class StoreAppMovieWebm
	{
		[BsonElement("480")]
		public string Webm480 { get; set; }

		[BsonElement("max")]
		public string WebmMax { get; set; }
	}

public class StoreAppMovieMp4
	{
		[BsonElement("480")]
		public string Mp4480 { get; set; }

		[BsonElement("max")]
		public string Mp4Max { get; set; }
	}

public class StoreAppReleaseDate
	{
		[BsonElement("coming_soon")]
		public bool ComingSoon { get; set; }

		[BsonElement("date")]
		public string Date { get; set; }
	}

public class StoreAppSupportInfo
	{
		[BsonElement("url")]
		public string Url { get; set; }

		[BsonElement("email")]
		public string Email { get; set; }
	}
	
	public class StoreAppContentDescriptors
	{
		[BsonElement("ids")]
		public int[] Ids { get; set; }

		[BsonElement("notes")]
		public string Notes { get; set; }
	}
	
	public class StoreAppGenre
	{
		[BsonElement("id")]
		public string Id { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }
	}
	
	public class StoreAppCategory
	{
		[BsonElement("id")]
		public string Id { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }
	}
	
	
	