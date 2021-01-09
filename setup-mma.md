# Setup 

Source: https://docs.microsoft.com/en-us/azure/storage/blobs/storage-upload-process-images?tabs=javascript

### Login
`az login --tenant 72f988bf-86f1-41af-91ab-2d7cd011db47`
`az account set --subscription 6ee947fa-0d77-4915-bf68-4a83a8bec2a4`

### Setup Storage account
```
az group create --name filequick --location northeurope

blobStorageAccount="stfilequick"

az storage account create --name $blobStorageAccount --location northeurope \
  --resource-group filequick --sku Standard_LRS --kind StorageV2 --access-tier hot


blobStorageAccountKey=$(az storage account keys list -g filequick \
  -n $blobStorageAccount --query "[0].value" --output tsv)

az storage container create --name images \
  --account-name $blobStorageAccount \
  --account-key $blobStorageAccountKey

az storage container create --name thumbnails \
  --account-name $blobStorageAccount \
  --account-key $blobStorageAccountKey --public-access container
```

### Setup WebApp
`az appservice plan create --name filequickAppServicePlan --resource-group filequick --sku Free`
`webapp="filequick"`

`az webapp create --name $webapp --resource-group filequick --plan filequickAppServicePlan`


### Deploy WebApp

**.NET**

Manual integration
```
az webapp deployment source config --name $webapp --resource-group filequick \
  --branch master --manual-integration \
  --repo-url https://github.com/michalmar/file-quick
```


Automatic integration

```
az webapp deployment source config --name $webapp --resource-group filequick \
  --branch master \
  --repo-url https://github.com/michalmar/file-quick
```

```
az webapp config appsettings set --name $webapp --resource-group filequick \
  --settings AzureStorageConfig__AccountName=$blobStorageAccount \
    AzureStorageConfig__ImageContainer=images \
    AzureStorageConfig__ThumbnailContainer=thumbnails \
    AzureStorageConfig__AccountKey=$blobStorageAccountKey
```
    
### Azure Functions
`az provider register --namespace Microsoft.EventGrid`

```
resourceGroupName="filequick"
location="northeurope"
functionstorage="stfilequickfunc"
az storage account create --name $functionstorage --location $location \
--resource-group $resourceGroupName --sku Standard_LRS --kind StorageV2
```


functionapp="funcquickfile"
az functionapp create --name $functionapp --storage-account $functionstorage \
  --resource-group $resourceGroupName --consumption-plan-location $location \
  --functions-version 2

storageConnectionString=$(az storage account show-connection-string --resource-group $resourceGroupName \
  --name $blobStorageAccount --query connectionString --output tsv)

az functionapp config appsettings set --name $functionapp --resource-group $resourceGroupName \
  --settings AzureWebJobsStorage=$storageConnectionString THUMBNAIL_CONTAINER_NAME=thumbnails \
  THUMBNAIL_WIDTH=100 FUNCTIONS_EXTENSION_VERSION=~2




NOT YET


az functionapp deployment source config --name $functionapp --resource-group $resourceGroupName \
  --branch master --manual-integration \
  --repo-url https://github.com/michalmar/file-quick/functions


  