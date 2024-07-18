
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

    Dictionary<string, int> _watchmanDict;
    Dictionary<string, int> _situations = [];
    Dictionary<string, int> _cards = [];

    public GameManager()
    {
        _secretHistoriesMainAssembly = MonoDomainGetMonoAssemblyByName(_monoRootDomain, "SecretHistories.Main");
        _secretHistoriesMainImage = MonoAssemblyGetMonoImage(_secretHistoriesMainAssembly);
        int watchman = MonoImageGetMonoClassByName(_secretHistoriesMainImage, "SecretHistories.UI", "Watchman");
        int vtable = MonoClassGetMonoVTable(_monoRootDomain, watchman);
        int staticFieldData = VTableGetStaticFieldData(vtable);
        int registered = ReadUnsafe<int>(staticFieldData);
        
        _watchmanDict = ReadDictionaryTypeObject(registered);

        UpdateTableTop();
        /*
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

    public void UpdateTableTop ( ) {
        int hornedaxe = _watchmanDict["HornedAxe"];
        Console.WriteLine($"HornedAxe : {hornedaxe}");
        // HashSet
        int registeredSpheres = ReadUnsafe<int>(hornedaxe + 0x18);
        int slotArray = ReadUnsafe<int>(registeredSpheres + 0xC);
        int sphereEntries = ReadUnsafe<int>(slotArray + 0xC);
        Console.WriteLine($"> Spheres : {sphereEntries}@{registeredSpheres}");
        for ( int sphereEntriesIt = 0 ; sphereEntriesIt < sphereEntries ; ++sphereEntriesIt ) {
            int sphere = ReadUnsafe<int>(slotArray + 0x18 + sphereEntriesIt * 0xC);
            if ( sphere == 0 )
                continue;

            int sphereVTable = ReadUnsafe<int>(sphere);
            int sphereClass = ReadUnsafe<int>(sphereVTable);
            string sphereClassName = MonoClassGetName(sphereClass);

            if ( sphereClassName == "TabletopSphere" ) {
                // List
                int tokenList = ReadUnsafe<int>(sphere + 0x34);
                int array = ReadUnsafe<int>(tokenList + 0x8);
                int tokenEntries = ReadUnsafe<int>(array + 0xC);
                Console.WriteLine($"> Tokens : {tokenEntries}@{tokenList}");
                for ( int tokenEntriesIt = 0 ; tokenEntriesIt < tokenEntries ; ++tokenEntriesIt ) {
                    int token = ReadUnsafe<int>(array + 0x10 + tokenEntriesIt * 0x4);
                    if ( token == 0 )
                        continue;

                    int payload = ReadUnsafe<int>(token + 0x24);
                    int payloadVTable = ReadUnsafe<int>(payload);
                    int payloadClass = ReadUnsafe<int>(payloadVTable);
                    string payloadClassName = MonoClassGetName(payloadClass);

                    if ( payloadClassName == "ElementStack" ) {
                        int id = ReadUnsafe<int>(payload + 0x10);
                        string idString = ReadUnsafeMonoWideString(id);
                        int element = ReadUnsafe<int>(payload + 0x14);
                        int label = ReadUnsafe<int>(element + 0x1C);
                        string labelString = ReadUnsafeMonoWideString(label);
                        int description = ReadUnsafe<int>(element + 0x20);
                        string descriptionString = ReadUnsafeMonoWideString(description);
                        int quantity = ReadUnsafe<int>(payload + 0x40);
                        Console.WriteLine($"Stack({idString}): {quantity}x {labelString}");
                        _cards.Add(idString, payload);
                    }
                    if ( payloadClassName == "Situation" ) {
                        int verb = ReadUnsafe<int>(payload + 0x20);
                        int label = ReadUnsafe<int>(verb + 0x1C);
                        string labelString = ReadUnsafeMonoWideString(label);
                        int id = ReadUnsafe<int>(payload + 0x24);
                        string idString = ReadUnsafeMonoWideString(id);
                        Console.WriteLine($"Situation({idString}): {labelString}");
                        _situations.Add(idString, payload);
                    }
                }
                break;
            }
        }
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