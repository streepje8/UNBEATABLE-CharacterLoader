# UNBEATABLE Character Loader
This mods allows the loading of custom characters in UNBEATABLE via `.ubcharacter` files.
The mod is partiaclly based of its predecessor made by Zachava96, please check them out at https://github.com/Zachava96/CustomCharacters

## Creating Characters
To create your own characters in the `.ubcharacter` format, please follow the instructions found at https://github.com/streepje8/UNBEATABLE-CharacterLoader-Template to modify the template.


## Installing the mod
In the future this mod will be available via UBMT (UNBEATABLE Modding Tool). Until then you can manually install the mod with the following steps:

1. Download and install BepInEx (https://github.com/BepInEx/BepInEx)
2. Run the game at least once
3. Download or build the dll of this mod 
4. Put the file in the `BepInEx\plugins` folder next to your game
5. Run the game, a new Characters folder should now appear next to your UNBEATABLE.exe, you can put `.ubcharacter` files in here and after a restart they should appear in game.

## Build instructions

Edit the .csproj file to change the <UnbeatableLocation> property
```
<UnbeatableLocation>C:\PATH_TO_YOUR\SteamLibrary\steamapps\common\UNBEATABLE</UnbeatableLocation>
```

Open the .csproj in an IDE of your choice (I Personally use rider)

Either in the CLI run
`dotnet restore`
`dotnet build`

or build in your IDE

The mod dll will be outputted (with other dll's that can be ignored) in the bin folder.