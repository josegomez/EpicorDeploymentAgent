# EpicorDeploymentAgent
Windows Service which will deploy CAB files dropped in a specific folder into several (configured) Epicor environments.

# Configuration
App.Config Settings
* DropPath: Path of Folder to monitor for CAB files
* EpicorPassword: password to be used to authenticate against epicor
* EpicorUser: Epicor password used to authenticate against Epicor
* EpicorCompany: Company for which to install the cab, blank means global
* EpicorSysConfigs: List of config files for each environment where you'd like to deploy the given CAB
* EpicorSolutionPath: Path to Solution.exe within the Epicor Client Folder

Nlog.config
Configure buffered email to receive a copy of the log after the cab is installed.
