{ pkgs, ... }: {
  # Canal de paquetes de NixOS
  channel = "stable-23.11"; 

  # Aquí le decimos a la máquina virtual qué programas instalar
  packages = [
    pkgs.dotnet-sdk_8
    pkgs.google-cloud-sdk
    pkgs.google-cloud-sql-proxy
  ];

  # Variables de entorno (vacías por ahora)
  env = {};
  
  idx = {
    # Extensiones del editor para ayudarte con C#
    extensions = [
      "muhammad-sammy.csharp"
    ];
  };
}