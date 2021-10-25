# MSI-Validator
Azure App Services supports an interesting feature called **Manage Identity** from Azure Active Directory. <br/>
This allows your App Services to easily connect to Azure Resources such as Azure KeyVault, Azure Storage, Azure SQL . <br/>
The complete list of resources that support this feature are available in the following document:<br/>
[Azure Services that support managed identities - Azure AD | Microsoft Docs](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/services-support-managed-identities)<br/> 

You could refer to our documentation [here](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=dotnet) for more details on this feature . <br/>

MSI-Validator helps you troubleshoot issues with Managed Identity for Azure App Services. <br/>
The link to download this tool is available in the attachments section of [this](https://techcommunity.microsoft.com/t5/apps-on-azure/managed-identity-for-azure-app-services/ba-p/1711597) blog.

## Installation Steps:
1. Download the zip file from the attachments.
Current version - v1.0.0.0
2. Extract it to the local folder.
3. Drag and drop "msi-validator.exe" to the Kudu console of the App Service [(https://<webapp-name>.scm.azurewebsites.net)]()
   ![](https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/01-%20Download%20and%20installation.png)

## Commands:
1. To Get an Access Token:
   > ```bash
   > msi-validator get-token -r <resource>​
   > ```
   > **Valid Arguments for resource = keyvault, storage, sql** <br>
   >
   > - msi-validator get-token -r keyvault
   > - msi-validator get-token -r storage
   > - msi-validator get-token -r sql
   
2. To Test the connection
   > ```bash
   > msi-validator test-connection -r < resource > -e < endpoint >
   > ```
   > **Valid Arguments for resource = keyvault, storage, sql** <br/>
   >
   > - msi-validator test-connection -r "keyvault" -e "https://< keyvault-name >.vault.azure.net/secrets/< secret-key >"
   > - msi-validator.exe test-connection -r storage -e https://< storage-name >.blob.core.windows.net/< container-name >/< blob-path >
   > - msi-validator.exe test-connection -r sql -e "Data Source=< server-name >.database.windows.net;Initial Catalog=< database-name >;"

## Troubleshooting :
1. From the Identity Blade of the App Service, ensure that Managed Identity is turned on. <br/>
   ![](https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/00%20-%20check%20in%20azure%20portal.png)
2. Navigate to Kudu Console (https://<webapp-name>.scm.azurewebsites.net) > Environment Section and search for MSI (Ctrl + F) <br/>
   ![](https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/02%20-%20check%20env%20variables.png)
   The Environmental Variables **MSI_ENDPOINT** and **MSI_SECRET** would have been set automatically.
3. Run the command **"msi-validator get-token -r < resource >"** and check if a token is being returned. <br/>
   ![](https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/03%20-%20inspect%20the%20token.png) <br/>
   An access token should be returned.
   Otherwise, it indicates that MSI service has issues reaching out to Azure Active Directory to fetch a token.
   >
   > **Things to check :**
   > 
   > - Does the App Service have regional VNet Integration / is the App in ASE?
   > Are there any User Defined Routes on the subnet to which the App Service is integrated ?
   > If Yes , is the device to which the traffic is force tunneled, blocking any Azure Active Directory Dependency ?
   > - Do you still face the issue if Managed Identity is disabled and enabled again ?
4. Run the command **"msi-validator test-connection -r < resource > -e < endpoint > "** and check if data is returned from the resource or inspect the error message.
   
   #### KeyVault <br/>
   From the below error message, we see that the App Service doesn’t have necessary permissions to access the KeyVault. <br/>
   ![](https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/04%20-%20keyvault%20issue.png)
   
   **Resolution :** </br>
    <ol type="a">
      <li>Navigate to the Access Policies Blade of KeyVault from the Azure Portal.</li>
      <li>Click on "+ Add Access Policy" <br>
          <img src=https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/05%20-%20keyvault%20select%20access%20policies.png>
      </li>
      <li>Provide the necessary permission. <br/>
          <img src=https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/06%20-%20select%20permissions.png>
      </li>
      <li>Choose the Service Principal (name of the App Service) <br/>
          <img src=https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/07%20-%20select%20service%20principal.png>
      </li>
    </ol> 
    
    #### Storage <br/>
    ![](https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/08%20-%20storage%20issue.png)
    
    **Resolution:**  <br/>
    Navigate to the Access Control IAM) > Add Role assignment and choose the necessary storage related permission. <br/>
    The roles should be configured as per your application's use case.
    ![](https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/09%20-%20storage%20role%20assginments.png)
    
    #### SQL <br/>
    The application could fail while connecting to Azure SQL using MSI with the error message: 
    **"Unable to connect to SQL. Exception : Login failed for user '<token-identified principal>'"**
    
    ![](https://github.com/vijaysaayi/MSI-Validator/blob/master/Images/10%20-%20sql%20issue.png)
    
    **Resolution :** <br/>
    > If you want, you can add the identity to an Azure AD group, then grant SQL Database access to the Azure AD group instead of the identity. 
    > For example, the following commands add the managed identity from the previous step to a new group called myAzureSQLDBAccessGroup:
    > ```bash
    > groupid=$(az ad group create --display-name myAzureSQLDBAccessGroup --mail-nickname myAzureSQLDBAccessGroup --query objectId --output tsv)
    > msiobjectid=$(az webapp identity show --resource-group myResourceGroup --name <app-name> --query principalId --output tsv)
    > az ad group member add --group $groupid --member-id $msiobjectid
    > az ad group member list -g $groupid
    > ```
    
    In the Cloud Shell, sign in to SQL Database by using the SQLCMD command. Replace <server-name> with your server name, <db-name> with the database name your app uses, and <aad-user-name> and <aad-password> with your Azure AD user's credentials.
    ```bash
    sqlcmd -S <server-name>.database.windows.net -d <db-name> -U <aad-user-name> -P "<aad-password>" -G -l 30
    ```
    
    In the SQL prompt for the database you want, run the following commands to grant the permissions your app needs. For example,
    ```SQL
    CREATE USER [<identity-name>] FROM EXTERNAL PROVIDER;
    ALTER ROLE db_datareader ADD MEMBER [<identity-name>];
    ALTER ROLE db_datawriter ADD MEMBER [<identity-name>];
    ALTER ROLE db_ddladmin ADD MEMBER [<identity-name>];
    GO
    ```
    
    **< identity-name >** is the name of the managed identity in Azure AD. If the identity is system-assigned, the name always the same as the name of your App Service app. To grant permissions for an Azure AD group, use the group's display name instead (for example, myAzureSQLDBAccessGroup).

    Type EXIT to return to the Cloud Shell prompt. </br>
    
    The back-end services of managed identities also maintains a token cache that updates the token for a target resource only when it expires. If you make a mistake configuring your SQL Database permissions and try to modify the permissions after trying to get a token with your app, you don't actually get a new token with the updated permissions until the cached token expires.


    **Modify connection string**

    Remember that the same changes you made in Web.config or appsettings.json works with the managed identity, so the only thing to do is to remove the existing connection string in App Service, which Visual Studio created deploying your app the first time. Use the following command, but replace <app-name> with the name of your app.
    
    ```bash
    az webapp config connection-string delete --resource-group myResourceGroup --name <app-name> --setting-names MyDbConnection
    ```
    
    You could refer to our documentation regarding this at [Tutorial: Access data with managed identity - Azure App Service | Microsoft Docs](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-connect-msi#grant-permissions-to-managed-identity)
    
## Download Link :
- **Option 1:** https://techcommunity.microsoft.com/t5/apps-on-azure/managed-identity-for-azure-app-services/ba-p/1711597
- **Option 2:** [Click here]()
   
    
  
    
