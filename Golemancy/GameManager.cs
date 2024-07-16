
namespace Golemancy;

public class GameManager : ProcessManager
{

    int _secretHistoriesMainAssembly;
    int _secretHistoriesMainImage;

    List<string> singletons = [
        "HornedAxe", // Sphere container
        "SecretHistory", // Debug log and tool; CTRL backtick
        "Numa", // Otherworld controller
        "DealersTable", // DrawPiles container
        "Xamanek", // Contains Itineraries
        "Meniscate", // Tabletop selection management
        "Heart", // Timer
        "Notifier", // Notifications
        "LocalNexus",
        "HintPanel", // Show hint
        "Stable", // Character container
        "PrefabFactory",
        "Compendium", // Entity stores
        "StageHand", // Scene manager
        "Limbo", // Special void sphere

        "IDice",
        "AchievementsChronicler",
        "SituationUIStyle",
        "ModManager",
        "LanguageManager",
        "ScreenResolutionAdapter",
        "NullManifestation",
        "BackgroundMusic",
        "CameraDragRect",
        "CamOperator",
        "AbstractBackground",
        "TabletopImageBurner",
        "IChronicler",
        "StatusBar",
        "TabletopFadeOverlay",
        "Autosaver",
        "GameGateway", // Start load end game
        "Concursum", // Notification and Culture
        "Config",
        "MetaInfo", // Version number, etc
        "StorefrontServicesProvider", // Steam interaction
    ];

    public GameManager()
    {
        _secretHistoriesMainAssembly = MonoDomainGetMonoAssemblyByName(_monoRootDomain, "SecretHistories.Main");
        _secretHistoriesMainImage = MonoAssemblyGetMonoImage(_secretHistoriesMainAssembly);
        int watchman = MonoImageGetMonoClassByName(_secretHistoriesMainImage, "SecretHistories.UI", "Watchman");
        int vtable = MonoClassGetMonoVTable(_monoRootDomain, watchman);
        int staticFieldData = VTableGetStaticFieldData(vtable);
        int registered = ReadUnsafe<int>(staticFieldData);
        
        var watchmanDict = ReadDictionaryTypeObject(registered);

        foreach (var (item, loc) in watchmanDict)
        {
            if (!singletons.Contains(item))
                Console.WriteLine($"{item}': {loc}");
            int somevtable = ReadUnsafe<int>(loc);
            int someclass = ReadUnsafe<int>(somevtable);
            Console.WriteLine($"{item}: {MonoClassGetNamespace(someclass)}.{MonoClassGetName(someclass)}");
        }
        /*
        Concursum: 452329072
            popup manager 1
Watchman.Get<Concursum>().ShowNotification(new NotificationArgs("THIS WAS A FREE COPY FOR BETA TESTING ONLY:( ", "BETA HAS EXPIRED :( CAT IS SAD :( PURCHASING & UPDATING THE GAME WILL MAKE THIS MESSAGE GO AWAY :) "));

foreach (Situation registeredSituation in Watchman.Get<HornedAxe>().GetRegisteredSituations())
		{
			if (registeredSituation.IsOpen && registeredSituation.StateIdentifier == StateEnum.Unstarted)
			{
				registeredSituation.TryStart();
				break;
			}
		}
        
				registeredSituation.Conclude();

                Character character = Watchman.Get<Stable>().Protag();

        HornedAxe: 452462104
        SecretHistory: 452329408
        MetaInfo: 452093920
        StorefrontServicesProvider: 452065280
        Config: 452124152
        SituationUIStyle: 153111104
        StageHand: 452461584
        Limbo: 153104032
        IDice: 452098672
        AchievementsChronicler: 452063968
        ModManager: 452164584
        Compendium: 453095968
        LanguageManager: 452109688
        PrefabFactory: 630665360
        Stable: 452478920
        ScreenResolutionAdapter: 452307136
        HintPanel: 452064192
        LocalNexus: 453148920
        BackgroundMusic: 597120992
        NullManifestation: 597360976

        HornedAxe: 452462104
SecretHistory: 452329408
MetaInfo: 452093920
StorefrontServicesProvider: 452065280
Config: 452124152
Concursum: 452329072
SituationUIStyle: 153111104
StageHand: 452461584
Limbo: 153104032
IDice: 452098672
AchievementsChronicler: 452063968
ModManager: 452164584
Compendium: 453095968
LanguageManager: 452109688
PrefabFactory: 630665360
Stable: 452478920
ScreenResolutionAdapter: 452307136
HintPanel: 452064192
LocalNexus: 453500640
BackgroundMusic: 598032384
NullManifestation: 597360976
Notifier: 609001952
CamOperator: 554467096
CameraDragRect: 600619648
AbstractBackground: 630684064
Heart: 630603680
Meniscate: 553714752
TabletopImageBurner: 584457944
IChronicler: 603513600
Xamanek: 584454624
StatusBar: 630683360
TabletopFadeOverlay: 597347760
Autosaver: 600619592
DealersTable: 600619536
GameGateway: 593822752
Numa: 587593864

        Watchman.Get<Compendium>().GetSingleEntity<Dictum>()

        public void CreateSituation()
	{
		SerializationHelper serializationHelper = new SerializationHelper();
		if (serializationHelper.MightBeJson(input.text))
		{
			serializationHelper.DeserializeFromJsonString<TokenCreationCommand>(input.text).Execute(Context.Unknown(), _situationDrydock);
			return;
		}
		string text = input.text;
		Compendium compendium = Watchman.Get<Compendium>();
		Recipe entityById = compendium.GetEntityById<Recipe>(text.Trim());
		SituationCreationCommand situationCreationCommand;
		if (entityById.IsValid())
		{
			situationCreationCommand = new SituationCreationCommand().WithVerbId(entityById.ActionId).WithRecipeAboutToActivate(entityById.Id);
			situationCreationCommand.TimeRemaining = entityById.Warmup;
		}
		else
		{
			if (compendium.GetEntityById<Verb>(text).Spontaneous)
			{
				NoonUtility.LogWarning("Trying to create an token for a non-existing, or spontaneous verb '" + text + "'");
				return;
			}
			situationCreationCommand = new SituationCreationCommand().WithVerbId(text);
		}
		TokenLocation location = new TokenLocation(0f, 0f, 0f, _situationDrydock.GetAbsolutePath());
		Token token = new TokenCreationCommand(situationCreationCommand, location).Execute(new Context(Context.ActionSource.Debug), _situationDrydock);
		EncaustDrydockedItem(token);
		PopulateLinksPanel(token.Payload);
	}

    public void AdvanceTimeToEndOfRecipe()
	{
		Token token = _situationDrydock.GetTokens().FirstOrDefault();
		if (token.Payload != null)
		{
			float lifetimeRemaining = token.Payload.GetTimeshadow().LifetimeRemaining;
			if (lifetimeRemaining > 0f)
			{
				_situationDrydock.RequestTokensSpendTime(lifetimeRemaining, 0f);
			}
		}
	}


private void CreateElementFromBestGuessOfElementId()
	{
		string text = input.text;
		if (Watchman.Get<Compendium>().GetEntityById<Element>(text).Id == NullElement.Create().Id)
		{
			return;
		}
		new Context(Context.ActionSource.Debug);
		IEnumerable<Token> tokens = _elementDrydock.GetTokens();
		Token token = null;
		foreach (Token item in tokens)
		{
			if (item.IsValidElementStack() && item.Payload.EntityId == text)
			{
				token = item;
			}
		}
		if (token != null)
		{
			token.Payload.ModifyQuantity(1);
		}
		else
		{
			_elementDrydock.ProvisionElementToken(text, 1);
		}
		EncaustDrydockedItem(_elementDrydock.GetTokens().FirstOrDefault());
	}

public void DoHeartbeats(int beatsToDo)
	{
		Watchman.Get<Heart>().Metapause();
		for (int i = 0; i < beatsToDo; i++)
		{
			Watchman.Get<Heart>().Beat(1f, 0f);
		}
		Watchman.Get<Heart>().Unmetapause();
	}
    
        */
    }

    /// <summary>
    /// HornedAxe contains all the Spheres
    /// </summary>
    public void HornedAxe()
    {

    }
    /// <summary>
    /// Numa controls going to Otherworlds
    /// </summary>
    public void Numa()
    {}

    /// <summary>
    /// DealersTable is the AbstractDominion that contains the DrawPile Spheres
    /// </summary>
    public void DealersTable()
    {}

    /// <summary>
    /// Everything in the game can be reached following a FucinePath; with the FucineRoot at root.
    /// </summary>
    public void Everything()
    {}
}