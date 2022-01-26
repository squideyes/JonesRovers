![License](https://img.shields.io/github/license/squideyes/Trading)

**JonesRovers** has been designed to be a good representative stand-in for some of the CI/CD research related to Edward Jones "Sherpa" effort.

|Project|Description|
|---|---|
|**Common**|A collection of common models and helper classes|
|**BackEnd**|A containerized Function App that simulates a moderately complex Azure-deployed back-end, including connectivity to both "OnPrem" and Azure-PaaS services, plus a timer-driven data collector|
|**FrontEnd**|A containerized Blazor website to exercise the BackEnd service|
|**OnPrem**|A containerized Function App, with a single  HttpTrigger, that is meant to be deployed on-prem and interoperate with the BackEnd service|

Be forewarned that literally no effort has been made to make the various classes and methods suitable for a general audience.
