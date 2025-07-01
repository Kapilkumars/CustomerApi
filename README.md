# Customer Management API / Jobs

## Health

Customer API
|   | Swagger | Build | Deploy | Health
| --- | --- | --- | --- | -- 
| Dev | [Link](https://api-dev.digitaldotsinc.com/customers/swagger/index.html)  | [![Build Status](https://dev.azure.com/digitaldotsinc/DigitalDots/_apis/build/status%2FCustomer%20Api%20Dev?repoName=DigitalDots-DevTeam%2FCustomerManagement&branchName=main)](https://dev.azure.com/digitaldotsinc/DigitalDots/_build/latest?definitionId=15&repoName=DigitalDots-DevTeam%2FCustomerManagement&branchName=main) | [![Release Status](https://vsrm.dev.azure.com/digitaldotsinc/_apis/public/Release/badge/f08950e6-8304-4e24-9025-4ae365a784f4/6/6)](https://dev.azure.com/digitaldotsinc/DigitalDots/_release?_a=releases&view=mine&definitionId=6) | [Dashboard](https://portal.azure.com/#@digitaldotsinc.com/dashboard/arm/subscriptions/e6c2b849-fc99-44a7-b027-515ea0f0303f/resourcegroups/dashboards/providers/microsoft.portal/dashboards/78010b0e-44e8-4c38-808f-a4045ab18299)
| QA | [Link](https://api-qa.digitaldotsinc.com/customers/swagger/index.html)  | [![Build Status](https://dev.azure.com/digitaldotsinc/DigitalDots/_apis/build/status%2FCustomer%20API%20QA?repoName=DigitalDots-DevTeam%2FCustomerManagement&branchName=QA)](https://dev.azure.com/digitaldotsinc/DigitalDots/_build/latest?definitionId=30&repoName=DigitalDots-DevTeam%2FCustomerManagement&branchName=QA) | [![Release Status](https://vsrm.dev.azure.com/digitaldotsinc/_apis/public/Release/badge/f08950e6-8304-4e24-9025-4ae365a784f4/14/14)](https://dev.azure.com/digitaldotsinc/DigitalDots/_release?_a=releases&view=mine&definitionId=14) | [Dashboard](https://portal.azure.com/#@digitaldotsinc.com/dashboard/arm/subscriptions/e6c2b849-fc99-44a7-b027-515ea0f0303f/resourcegroups/metiscloudqa/providers/microsoft.portal/dashboards/19d3bea7-9004-4824-b092-f55eec9ce89b)
| Feature Branches | [elitsa](https://digitaldotscustomerapidev-elitsa.azurewebsites.net/swagger) [guy](https://digitaldotscustomerapidev-guy.azurewebsites.net/swagger) [svet](https://digitaldotscustomerapidev-svet.azurewebsites.net/swagger) [olek](https://digitaldotscustomerapidev-olek.azurewebsites.net/swagger)| [![elitsa](https://dev.azure.com/digitaldotsinc/DigitalDots/_apis/build/status%2FPR%2FCustomer%20Api%20PR?repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=elitsa)](https://dev.azure.com/digitaldotsinc/DigitalDots/_build/latest?definitionId=40&repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=elitsa) <br>[![guy](https://dev.azure.com/digitaldotsinc/DigitalDots/_apis/build/status%2FPR%2FCustomer%20Api%20PR?repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=elitsa)](https://dev.azure.com/digitaldotsinc/DigitalDots/_build/latest?definitionId=40&repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=elitsa) <br>[![olek](https://dev.azure.com/digitaldotsinc/DigitalDots/_apis/build/status%2FPR%2FCustomer%20Api%20PR?repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=elitsa)](https://dev.azure.com/digitaldotsinc/DigitalDots/_build/latest?definitionId=40&repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=elitsa) <br>[![svet](https://dev.azure.com/digitaldotsinc/DigitalDots/_apis/build/status%2FPR%2FCustomer%20Api%20PR?repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=elitsa)](https://dev.azure.com/digitaldotsinc/DigitalDots/_build/latest?definitionId=40&repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=elitsa) | [![Release Status](https://vsrm.dev.azure.com/digitaldotsinc/_apis/public/Release/badge/f08950e6-8304-4e24-9025-4ae365a784f4/23/23)](https://dev.azure.com/digitaldotsinc/DigitalDots/_release?view=all&_a=releases&definitionId=23) | TBD
| Prod | [Link](https://api.digitaldotsinc.com/customers/swagger/index.html) | [![Build Status](https://dev.azure.com/digitaldotsinc/DigitalDots/_apis/build/status%2FProd%2FCustomer%20API%20Prod?repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=prod)](https://dev.azure.com/digitaldotsinc/DigitalDots/_build/latest?definitionId=59&repoName=DigitalDots-DevTeam%2Fcustomer-api&branchName=prod) | TBD | TBD

Customer Jobs


## GitHub instructions
There are 3 team branches: **Main, QA,** and **Prod**.


All developers should create their branches based on main. When the work is complete, the work branch is merged into “main”. This is where other developers can pull the latest code by other engineers. 

When code is tested and it works without regressions, the “main” branch is pushed to the “QA” branch. In the QA branch, all engineers assume that the code is stable and can be used for local and field deployments for testing purposes, including automation. The “QA” branch is pushed to production on sprint (2 – 4 -week bases). 

The “Prod” branch is the most stable branch and is used for customer facing deployments. There are no feature or bug branches based on “Prod” or “QA”. The only away for code to flow is personName-issueName-dateCreated -> main -> QA -> Prod

Each issue should first be created in Jira and then connected to the GitHub as a branch, following the above convention.

> **Example:**
> Creating the branch `tristan-chilvers_PMM-137-create-base-empty-table-and-search-bar_2022-10-09`
> <br><br>
> <img src="https://github.com/DigitalDots-DevTeam/misc/blob/main/JiraMakeBranch.gif" width="900" />
> <img src="https://github.com/DigitalDots-DevTeam/misc/blob/main/UploadBranch.gif" width="900" />

# Training

All developers are required to complete the training that relates to their job roles and to do overview on the Coding Practices section. Certifications will be planed and added to the road map as required training, but they can span on longer time periods. 

[End User Setup Instructions](https://github.com/GuyDigital/DigitalDotsServer/wiki/End-User-Setup-Instructions)

Google Docs Links:
* [Home](https://drive.google.com/drive/folders/1w9fztcR_POV3pydEdqqKOhG0hx2H1BcP)
* [Xicato API Docs](https://drive.google.com/drive/folders/1h3tmGwAuOxDqbjq24RieVxos_WdXjYqj)
* [MISC DOCS](https://drive.google.com/drive/folders/1BOey0hSQcl4M8wp_BkyOi_r-nCu3JUKt)
* [Meeting Recordings](https://drive.google.com/drive/folders/169IQXfMw2Y7IHDvDT8HEAbMz8Z3Cj-Sl)
* [Figma](https://www.figma.com/file/M8WTLaNl5gcfLzuJzvR0qW/Metis-UI?node-id=6%3A2)

# Local Env Setup Process
## Backend API


Recommendations:

Add the following configuration: Tools/Options/TextEditor/C#/CodeStyles/General/NamespaceDeclaration -> File-Scoped
Documentation can be light and few endpoints can be called without an object id reference. Use Metis-Cloud-Dev to see how the apis are used and find objects to query.
## Front React App

[Cloud Portal](https://github.com/DigitalDots-DevTeam/cloud-portal/blob/main/README.md)

# CI CD Process

## Front-End

### Dev
* [Azure DevOps Pipeline](https://dev.azure.com/digitaldotsinc/DigitalDots/_build?definitionId=5)
* [Deployed Application](https://icy-wave-08b1b6210.3.azurestaticapps.net/)

### QA
* [Azure DevOps Pipeline](https://dev.azure.com/digitaldotsinc/DigitalDots/_build?definitionId=6)
* [Deployed Application](https://gray-hill-057318110.3.azurestaticapps.net/)

The DevOps Pipelines are configured to watch specific Github branches (currently `main` for dev and `QA` for QA). Any merges to these branches will trigger a build and deploy of the respective front-end application.

### Modifying the build process

#### Triggers
The above Azure DevOps Pipeline may be edited to modify settings. Once you are editing the pipeline:

1. Click the Triggers tab
1. Select the Continuous Integration trigger
1. Add/remove branches that will be monitored for deployment

#### Environment Variables
The above Azure DevOps Pipeline may be edited to modify settings. Once you are editing the pipeline:

1. Click the Variables tab
1. Add/edit/remove environment variables

#### Build Steps
The above Azure DevOps Pipeline may be edited to modify settings. Once you are editing the pipeline:

1. Click the Tasks tab
1. The Get Sources task fetches the source code from GitHub. This should be edited to match the branch monitored by the trigger
1. Agent Job 1 specifies the agent that will run the tasks attached to it
1. Static Web App: Dashboard is the build and deploy step. This may be modified if/when the steps to build the application change

![](https://github.com/DigitalDots-DevTeam/DigitalDotsServer/blob/main/IoT%20Components%20Architecture%20-Copy%20of%20Page-1.jpg)
