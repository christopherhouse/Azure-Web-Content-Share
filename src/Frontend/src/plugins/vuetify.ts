import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { mdi } from 'vuetify/iconsets/mdi-svg'
import * as mdiIcons from '@mdi/js'

// Import Vuetify styles
import 'vuetify/styles'

export default createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi',
    sets: {
      mdi: mdi,
    },
    aliases: {
      // Common icons we'll use
      upload: mdiIcons.mdiUpload,
      download: mdiIcons.mdiDownload,
      share: mdiIcons.mdiShare,
      delete: mdiIcons.mdiDelete,
      edit: mdiIcons.mdiPencil,
      user: mdiIcons.mdiAccount,
      users: mdiIcons.mdiAccountGroup,
      admin: mdiIcons.mdiAccountCog,
      file: mdiIcons.mdiFile,
      folder: mdiIcons.mdiFolderOpen,
      security: mdiIcons.mdiSecurity,
      logout: mdiIcons.mdiLogout,
      login: mdiIcons.mdiLogin,
      cloud: mdiIcons.mdiCloud,
      link: mdiIcons.mdiLink,
      email: mdiIcons.mdiEmail,
      calendar: mdiIcons.mdiCalendar,
      check: mdiIcons.mdiCheck,
      close: mdiIcons.mdiClose,
      info: mdiIcons.mdiInformation,
      warning: mdiIcons.mdiAlert,
      error: mdiIcons.mdiAlertCircle,
      success: mdiIcons.mdiCheckCircle,
    },
  },
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          primary: '#1976D2', // Azure blue
          secondary: '#424242',
          accent: '#82B1FF',
          error: '#FF5252',
          info: '#2196F3',
          success: '#4CAF50',
          warning: '#FFC107',
          surface: '#FAFAFA',
          background: '#FFFFFF',
        },
      },
      dark: {
        colors: {
          primary: '#2196F3',
          secondary: '#424242',
          accent: '#FF4081',
          error: '#FF5252',
          info: '#2196F3',
          success: '#4CAF50',
          warning: '#FB8C00',
          surface: '#1E1E1E',
          background: '#121212',
        },
      },
    },
  },
})