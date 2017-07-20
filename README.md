# Azure-AS-Tracer

A windows service / console application to create & subscribe to Analysis Services XEvents and save them on disk in JSON format.

Although it was created for Azure Analysis Services it also work well with SQL Server Analysis Services.

## Install & Run

* Download \dist folder

* Run 'setup.install.bat' with 'run as administrator'

* Change the Analysis Services connection string on 'AzureASTrace.exe.config'

* Start the Windows Service

## Run in Console Mode

Simply execute AzureASTrace.exe

## Output

Trace events will be stored in the output folder (configurable) as JSONL files organized by Date and Event Type.

