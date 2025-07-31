<template>
  <v-container fluid class="privacy-container">
    <!-- Hero Section -->
    <v-row class="hero-section" no-gutters>
      <v-col cols="12">
        <div class="hero-content text-center">
          <h1 class="display-1 font-weight-bold mb-4 text-white">
            🔒 Privacy Policy
          </h1>
          <p class="headline text-white mb-6 opacity-90">
            Your privacy and data security are our highest priorities
          </p>
        </div>
      </v-col>
    </v-row>

    <!-- Main Content -->
    <v-row class="main-content" justify="center">
      <v-col cols="12" lg="10" xl="8">
        
        <!-- Introduction -->
        <v-card class="mb-6" elevation="8" rounded="xl">
          <v-card-text class="pa-8">
            <div class="text-center mb-6">
              <v-icon size="64" color="primary" class="mb-4">mdi-shield-check</v-icon>
              <h2 class="text-h4 font-weight-bold mb-4">🛡️ Built for Security & Privacy</h2>
              <p class="text-h6 text-medium-emphasis">
                Azure Web Content Share is designed with enterprise-grade security and privacy protections
                to ensure your files and data remain secure at all times.
              </p>
            </div>
          </v-card-text>
        </v-card>

        <!-- Data Protection Section -->
        <v-card class="mb-6" elevation="4" rounded="xl">
          <v-card-title class="pa-6 bg-primary text-white">
            <v-icon class="mr-3" size="large">mdi-database-lock</v-icon>
            <span class="text-h5">🔐 Data Protection & Encryption</span>
          </v-card-title>
          
          <v-card-text class="pa-6">
            <v-row>
              <v-col cols="12" md="6">
                <div class="protection-item mb-4">
                  <h4 class="text-h6 mb-2">
                    <v-icon color="success" class="mr-2">mdi-check-circle</v-icon>
                    Encryption at Rest
                  </h4>
                  <p class="text-body-1">
                    All files are encrypted using <strong>AES-256 encryption</strong> when stored in Azure Blob Storage.
                    Your data is protected even if physical storage is compromised.
                  </p>
                </div>
                
                <div class="protection-item mb-4">
                  <h4 class="text-h6 mb-2">
                    <v-icon color="success" class="mr-2">mdi-check-circle</v-icon>
                    Encryption in Transit
                  </h4>
                  <p class="text-body-1">
                    All data transfers use <strong>TLS 1.2+ encryption</strong> to protect your files
                    during upload, download, and all communications with our servers.
                  </p>
                </div>
              </v-col>
              
              <v-col cols="12" md="6">
                <div class="protection-item mb-4">
                  <h4 class="text-h6 mb-2">
                    <v-icon color="success" class="mr-2">mdi-check-circle</v-icon>
                    Secure Key Management
                  </h4>
                  <p class="text-body-1">
                    Encryption keys are managed by <strong>Azure Key Vault</strong> with hardware security modules (HSMs)
                    ensuring the highest level of key protection.
                  </p>
                </div>
                
                <div class="protection-item mb-4">
                  <h4 class="text-h6 mb-2">
                    <v-icon color="success" class="mr-2">mdi-check-circle</v-icon>
                    Data Residency
                  </h4>
                  <p class="text-body-1">
                    Your data remains within your selected Azure region and complies with local
                    data sovereignty requirements.
                  </p>
                </div>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- Authentication & Access Control -->
        <v-card class="mb-6" elevation="4" rounded="xl">
          <v-card-title class="pa-6 bg-info text-white">
            <v-icon class="mr-3" size="large">mdi-account-lock</v-icon>
            <span class="text-h5">🔑 Authentication & Access Control</span>
          </v-card-title>
          
          <v-card-text class="pa-6">
            <v-row>
              <v-col cols="12" md="6">
                <div class="auth-item mb-4">
                  <h4 class="text-h6 mb-2">
                    <v-icon color="info" class="mr-2">mdi-microsoft</v-icon>
                    Azure AD Integration
                  </h4>
                  <p class="text-body-1">
                    Authentication is handled by <strong>Azure Active Directory</strong>, providing enterprise-grade
                    identity management with multi-factor authentication support.
                  </p>
                </div>
                
                <div class="auth-item mb-4">
                  <h4 class="text-h6 mb-2">
                    <v-icon color="info" class="mr-2">mdi-shield-account</v-icon>
                    Role-Based Access Control (RBAC)
                  </h4>
                  <p class="text-body-1">
                    User permissions are managed through <strong>RBAC</strong> with clearly defined roles:
                    Content Owners can share files, Administrators can manage users and all content.
                  </p>
                </div>
              </v-col>
              
              <v-col cols="12" md="6">
                <div class="auth-item mb-4">
                  <h4 class="text-h6 mb-2">
                    <v-icon color="info" class="mr-2">mdi-clock-alert</v-icon>
                    Time-Limited Access
                  </h4>
                  <p class="text-body-1">
                    All shared files have <strong>configurable expiration dates</strong>. Access links
                    automatically expire, and files are securely deleted when no longer needed.
                  </p>
                </div>
                
                <div class="auth-item mb-4">
                  <h4 class="text-h6 mb-2">
                    <v-icon color="info" class="mr-2">mdi-key-variant</v-icon>
                    Secure Access Tokens
                  </h4>
                  <p class="text-body-1">
                    File access uses <strong>cryptographically secure tokens</strong> that cannot be
                    guessed or forged, ensuring only authorized users can access shared content.
                  </p>
                </div>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- Azure Service Protections -->
        <v-card class="mb-6" elevation="4" rounded="xl">
          <v-card-title class="pa-6 bg-warning text-white">
            <v-icon class="mr-3" size="large">mdi-cloud-lock</v-icon>
            <span class="text-h5">☁️ Azure Service Protections</span>
          </v-card-title>
          
          <v-card-text class="pa-6">
            <v-row>
              <v-col cols="12" md="4" v-for="protection in azureProtections" :key="protection.title">
                <v-card class="protection-card h-100" elevation="2" rounded="lg">
                  <v-card-text class="text-center pa-4">
                    <div class="protection-icon mb-3">{{ protection.icon }}</div>
                    <h4 class="text-h6 mb-2">{{ protection.title }}</h4>
                    <p class="text-body-2">{{ protection.description }}</p>
                  </v-card-text>
                </v-card>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- Data Collection & Usage -->
        <v-card class="mb-6" elevation="4" rounded="xl">
          <v-card-title class="pa-6 bg-success text-white">
            <v-icon class="mr-3" size="large">mdi-information</v-icon>
            <span class="text-h5">📋 Data Collection & Usage</span>
          </v-card-title>
          
          <v-card-text class="pa-6">
            <v-alert
              type="success"
              variant="tonal"
              class="mb-4"
              prominent
            >
              <template v-slot:prepend>
                <v-icon>mdi-shield-check</v-icon>
              </template>
              <div class="text-h6 mb-2">✨ Minimal Data Collection</div>
              <p class="mb-0">
                We collect only the essential information needed to provide our file sharing service.
                We never sell your data or use it for advertising purposes.
              </p>
            </v-alert>

            <h4 class="text-h6 mb-3">🔍 What We Collect:</h4>
            <v-list class="mb-4">
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-account</v-icon>
                </template>
                <v-list-item-title>Authentication Information</v-list-item-title>
                <v-list-item-subtitle>Your Azure AD identity (name, email) for authentication and authorization</v-list-item-subtitle>
              </v-list-item>
              
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-file</v-icon>
                </template>
                <v-list-item-title>File Metadata</v-list-item-title>
                <v-list-item-subtitle>File names, sizes, and sharing settings (but not file contents)</v-list-item-subtitle>
              </v-list-item>
              
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="primary">mdi-history</v-icon>
                </template>
                <v-list-item-title>Usage Logs</v-list-item-title>
                <v-list-item-subtitle>Access logs for security auditing and system improvement</v-list-item-subtitle>
              </v-list-item>
            </v-list>

            <h4 class="text-h6 mb-3">🚫 What We Don't Do:</h4>
            <v-list>
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="error">mdi-cancel</v-icon>
                </template>
                <v-list-item-title>No Content Scanning</v-list-item-title>
                <v-list-item-subtitle>We do not scan, analyze, or access your file contents</v-list-item-subtitle>
              </v-list-item>
              
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="error">mdi-cancel</v-icon>
                </template>
                <v-list-item-title>No Data Selling</v-list-item-title>
                <v-list-item-subtitle>Your data is never sold to third parties</v-list-item-subtitle>
              </v-list-item>
              
              <v-list-item>
                <template v-slot:prepend>
                  <v-icon color="error">mdi-cancel</v-icon>
                </template>
                <v-list-item-title>No Advertising</v-list-item-title>
                <v-list-item-subtitle>We don't use your data for targeted advertising</v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>

        <!-- Compliance & Certifications -->
        <v-card class="mb-6" elevation="4" rounded="xl">
          <v-card-title class="pa-6 bg-purple text-white">
            <v-icon class="mr-3" size="large">mdi-certificate</v-icon>
            <span class="text-h5">📜 Compliance & Certifications</span>
          </v-card-title>
          
          <v-card-text class="pa-6">
            <p class="text-body-1 mb-4">
              Azure Web Content Share inherits all compliance certifications and security standards from Microsoft Azure:
            </p>
            
            <v-row>
              <v-col cols="12" sm="6" md="4" v-for="compliance in complianceStandards" :key="compliance.name">
                <v-chip
                  class="ma-1"
                  color="purple"
                  variant="outlined"
                  size="large"
                >
                  {{ compliance.icon }} {{ compliance.name }}
                </v-chip>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>

        <!-- Contact & Questions -->
        <v-card class="mb-6" elevation="4" rounded="xl">
          <v-card-title class="pa-6">
            <v-icon class="mr-3" color="primary" size="large">mdi-help-circle</v-icon>
            <span class="text-h5">❓ Questions or Concerns?</span>
          </v-card-title>
          
          <v-card-text class="pa-6 text-center">
            <p class="text-h6 mb-4">
              We're committed to transparency and answering any privacy questions you may have.
            </p>
            <p class="text-body-1 mb-4">
              If you have questions about our privacy practices, data handling, or security measures,
              please don't hesitate to reach out to your system administrator or IT support team.
            </p>
            
            <v-alert
              type="info"
              variant="tonal"
              class="text-left"
            >
              <template v-slot:prepend>
                <v-icon>mdi-lightbulb</v-icon>
              </template>
              <div class="text-body-1">
                <strong>💡 Pro Tip:</strong> You can always view your shared files, check expiration dates,
                and manage access through your dashboard when signed in.
              </div>
            </v-alert>
          </v-card-text>
        </v-card>

      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
// Azure service protection features
const azureProtections = [
  {
    icon: '🔒',
    title: 'DDoS Protection',
    description: 'Built-in protection against distributed denial-of-service attacks to ensure service availability.'
  },
  {
    icon: '🛡️',
    title: 'Network Security',
    description: 'Virtual network isolation and firewall rules protect against unauthorized network access.'
  },
  {
    icon: '👁️',
    title: '24/7 Monitoring',
    description: 'Continuous security monitoring and threat detection by Microsoft Security Operations Center.'
  },
  {
    icon: '🔄',
    title: 'Automatic Backups',
    description: 'Geo-redundant storage ensures your data is automatically backed up across multiple regions.'
  },
  {
    icon: '⚡',
    title: 'Incident Response',
    description: 'Rapid response to security incidents with automated threat mitigation and remediation.'
  },
  {
    icon: '📊',
    title: 'Audit Logging',
    description: 'Comprehensive audit logs for compliance and security investigation purposes.'
  }
]

// Compliance standards and certifications
const complianceStandards = [
  { name: 'SOC 2 Type II', icon: '🏛️' },
  { name: 'ISO 27001', icon: '🌐' },
  { name: 'GDPR', icon: '🇪🇺' },
  { name: 'HIPAA', icon: '🏥' },
  { name: 'FedRAMP', icon: '🇺🇸' },
  { name: 'PCI DSS', icon: '💳' }
]
</script>

<style scoped>
.privacy-container {
  padding: 0;
  min-height: 100vh;
}

.hero-section {
  background: linear-gradient(135deg, #6A1B9A 0%, #8E24AA 50%, #9C27B0 100%);
  min-height: 40vh;
  display: flex;
  align-items: center;
  position: relative;
}

.hero-section::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><defs><pattern id="grain" width="100" height="100" patternUnits="userSpaceOnUse"><circle cx="50" cy="50" r="1" fill="white" opacity="0.1"/></pattern></defs><rect width="100" height="100" fill="url(%23grain)"/></svg>');
  opacity: 0.3;
}

.hero-content {
  position: relative;
  z-index: 1;
  padding: 2rem;
}

.main-content {
  margin-top: -80px;
  position: relative;
  z-index: 2;
  padding: 0 1rem;
}

.protection-item,
.auth-item {
  border-left: 4px solid var(--v-theme-primary);
  padding-left: 1rem;
}

.protection-card {
  transition: all 0.3s ease;
  border: 1px solid rgba(0,0,0,0.1);
}

.protection-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.15) !important;
}

.protection-icon {
  font-size: 2.5rem;
  line-height: 1;
}

@media (max-width: 960px) {
  .hero-section {
    min-height: 30vh;
  }
  
  .main-content {
    margin-top: -40px;
  }
  
  .hero-content {
    padding: 1rem;
  }
  
  .display-1 {
    font-size: 2rem !important;
  }
}
</style>