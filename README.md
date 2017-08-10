# Azure-AS-Tracer

A windows service / console application to create & subscribe to Analysis Services XEvents and save them on disk in JSON format.

Although it was created for Azure Analysis Services it also work well with SQL Server Analysis Services.

Usage info: https://devscopebi.wordpress.com/2017/07/20/azure-analysis-services-tracer-aka-azureastrace-exe/

## Install & Run

* Download \dist folder

* Run 'setup.install.bat' with 'run as administrator'

* Change the Analysis Services connection string on 'AzureASTrace.exe.config'

* Start the Windows Service

* Change the XEvent trace template - Optional

## Run in Console Mode

Simply execute AzureASTrace.exe

## Output

Trace events will be stored in the output folder (configurable) as JSONL files organized by Date and Event Type.

## Power BI

Use the Power BI Template (.\PowerBI\AzureASTrace.pbit) to analyse the trace output

![](https://github.com/DevScope/Azure-AS-Tracer/blob/master/images/PowerBI.DataFiles.png)

![](https://github.com/DevScope/Azure-AS-Tracer/blob/master/images/PowerBI.TemplateParameter.png)

![](https://github.com/DevScope/Azure-AS-Tracer/blob/master/images/PowerBI.Report1.png)

![](https://github.com/DevScope/Azure-AS-Tracer/blob/master/images/PowerBI.Report2.png)




