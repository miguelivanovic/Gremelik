{ pkgs, ... }: {
  # Usamos el canal estable de Nix
  channel = "stable-23.11"; 

  # Aquí instalamos .NET 8
  packages = [
    pkgs.dotnet-sdk_8
    pkgs.powershell
  ];

  # Configuraciones de IDX
  idx = {
    # Extensiones para Visual Studio Code (C#)
    extensions = [
      "muhammad-sammy.csharp"
    ];
    
    # Vista previa (opcional, por ahora vacía)
    previews = {};
  };
}