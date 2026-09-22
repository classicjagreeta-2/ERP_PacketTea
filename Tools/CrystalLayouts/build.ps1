param([string]$Source, [string]$Out)
$ErrorActionPreference = "Stop"
$g64 = "C:\Windows\Microsoft.NET\assembly\GAC_64"
$gm = "C:\Windows\Microsoft.NET\assembly\GAC_MSIL"
$v = "v4.0_13.0.4000.0__692fbea5521e1304"
$refs = New-Object System.Collections.Generic.List[string]
$refs.Add("/r:$gm\CrystalDecisions.CrystalReports.Engine\$v\CrystalDecisions.CrystalReports.Engine.dll")
$refs.Add("/r:$gm\CrystalDecisions.Shared\$v\CrystalDecisions.Shared.dll")
foreach ($n in "ClientDoc", "Controllers", "ReportDefModel", "DataDefModel", "CommonObjectModel", "CommLayer", "DataSetConversion", "ObjectFactory") {
    $refs.Add("/r:$g64\CrystalDecisions.ReportAppServer.$n\$v\CrystalDecisions.ReportAppServer.$n.dll")
}
$refs.Add("/r:System.Data.dll")
$refs.Add("/r:System.Xml.dll")
$csc = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
& $csc /nologo /platform:x64 "/out:$Out" $refs $Source
exit $LASTEXITCODE
