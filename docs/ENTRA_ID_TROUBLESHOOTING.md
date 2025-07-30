# Entra ID Authentication Troubleshooting Guide 🔍

This guide helps diagnose and resolve common Azure AD/Entra ID authentication issues with the Azure Web Content Share application.

## 🚨 Common Error Messages and Solutions

### Error: "Scope 'access_as_user' doesn't exist"

**Full Error Message:**
```
AADSTS650053: The application 'Azure Web Content Share Frontend' asked for scope 'access_as_user' that doesn't exist on the resource '[api-client-id]'. Contact the app vendor.
```

**Root Cause:**
The API app registration in Azure AD does not have the `access_as_user` scope exposed.

**Solution:**
1. Go to Azure Portal → Azure Active Directory → App registrations
2. Find your **API app registration** (not the frontend app)
3. Navigate to **"Expose an API"**
4. If no scopes exist, click **"Add a scope"**
5. Configure the scope:
   - **Scope name**: `access_as_user`
   - **Who can consent**: `Admins and users`
   - **Admin consent display name**: `Access Azure Web Content Share API`
   - **Admin consent description**: `Allow the application to access the API on the signed-in user's behalf`
   - **User consent display name**: `Access Azure Web Content Share API`
   - **User consent description**: `Allow the application to access the API on your behalf`
   - **State**: `Enabled`
6. Click **"Add scope"**

**Verification:**
After adding the scope, the Application ID URI should show as `api://[your-api-client-id]` and you should see the `access_as_user` scope listed.

### Error: "Invalid client" or "Client authentication failed"

**Solution:**
1. Verify the client IDs in your environment variables match the app registrations
2. For the API app: Check that client secret hasn't expired
3. For the frontend app: Ensure redirect URIs are correctly configured

### Error: "CORS policy" blocking requests

**Solution:**
1. Check the API configuration for allowed CORS origins
2. Ensure the frontend URL is included in the CORS configuration
3. Verify HTTPS is used in production

## 🔧 Diagnostic Steps

### Step 1: Verify App Registration Configuration

Run this PowerShell script to check your app registrations:

```powershell
# Install the Microsoft Graph PowerShell module if not already installed
Install-Module Microsoft.Graph -Scope CurrentUser

# Connect to Microsoft Graph
Connect-MgGraph -Scopes "Application.Read.All"

# Replace with your actual client IDs
$ApiClientId = "b38a7b4e-8af7-48cf-9939-93198412492b"
$FrontendClientId = "your-frontend-client-id"

# Check API app registration
$ApiApp = Get-MgApplication -Filter "appId eq '$ApiClientId'"
Write-Host "API App: $($ApiApp.DisplayName)"
Write-Host "API Scopes:" ($ApiApp.Api.Oauth2PermissionScopes | ForEach-Object { $_.Value })

# Check frontend app registration
$FrontendApp = Get-MgApplication -Filter "appId eq '$FrontendClientId'"
Write-Host "Frontend App: $($FrontendApp.DisplayName)"
Write-Host "Frontend Required Permissions:"
$FrontendApp.RequiredResourceAccess | ForEach-Object {
    Write-Host "  Resource: $($_.ResourceAppId)"
    $_.ResourceAccess | ForEach-Object {
        Write-Host "    Permission: $($_.Id) (Type: $($_.Type))"
    }
}
```

### Step 2: Test Token Acquisition

Use this test script to verify token acquisition:

```javascript
// Browser console test script
// Open browser dev tools on your frontend application and run:

const testAuth = async () => {
    try {
        const response = await fetch('/api/health', {
            headers: {
                'Authorization': `Bearer ${accessToken}` // Use actual token from auth store
            }
        });
        console.log('API Response:', response.status, await response.text());
    } catch (error) {
        console.error('API Test failed:', error);
    }
};

// Check current authentication state
console.log('Current auth state:', {
    isAuthenticated: useAuthStore().isAuthenticated,
    hasToken: !!useAuthStore().accessToken,
    user: useAuthStore().user
});
```

### Step 3: Inspect JWT Token

1. Copy your access token from the browser's local storage or network tab
2. Go to [jwt.ms](https://jwt.ms) and paste the token
3. Verify these claims:
   - **aud** (audience): Should be `api://[your-api-client-id]`
   - **scp** (scopes): Should include `access_as_user`
   - **iss** (issuer): Should be `https://sts.windows.net/[tenant-id]/`

## 📋 Configuration Checklist

### API App Registration
- [ ] App registration exists with correct name
- [ ] **"Expose an API"** section configured
- [ ] Scope `access_as_user` is created and enabled
- [ ] Application ID URI is set to `api://[client-id]`
- [ ] Client secret created and not expired (for development)

### Frontend App Registration  
- [ ] App registration exists with correct name
- [ ] **"API permissions"** includes the API app's `access_as_user` scope
- [ ] Admin consent granted for the API permission
- [ ] Redirect URIs configured for your frontend URLs
- [ ] Platform configuration set to "Single Page Application (SPA)"

### Environment Configuration
- [ ] `VITE_AZURE_CLIENT_ID` matches frontend app client ID
- [ ] `VITE_AZURE_TENANT_ID` matches your tenant ID
- [ ] `VITE_API_CLIENT_ID` matches API app client ID
- [ ] API backend has correct Entra ID configuration

## 🛠️ Quick Fix Commands

If you have Azure CLI access, these commands can help verify configuration:

```bash
# List app registrations
az ad app list --display-name "Azure Web Content Share" --query '[].{DisplayName:displayName, ClientId:appId}'

# Check specific app registration details
az ad app show --id "your-api-client-id" --query '{DisplayName:displayName, Api:api}'

# Check service principal permissions
az ad sp list --display-name "Azure Web Content Share" --query '[].{DisplayName:displayName, AppId:appId}'
```

## 📞 When to Contact Support

Contact your Azure administrator or Microsoft support if:
- You don't have permissions to modify app registrations
- The tenant requires admin approval for new scopes
- You're seeing consistent authentication failures after following all steps
- You suspect the issue is related to Conditional Access policies

## 🔗 Useful Links

- [Microsoft Identity Platform Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [MSAL.js Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications)
- [Azure AD Error Codes](https://docs.microsoft.com/en-us/azure/active-directory/develop/reference-aadsts-error-codes)
- [JWT Token Inspector](https://jwt.ms)