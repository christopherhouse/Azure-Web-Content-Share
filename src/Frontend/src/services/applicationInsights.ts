import { ApplicationInsights } from '@microsoft/applicationinsights-web'

export class AppInsightsService {
  private appInsights: ApplicationInsights | null = null

  public initialize(connectionString: string): void {
    if (!connectionString) {
      console.warn('Application Insights connection string not provided')
      return
    }

    this.appInsights = new ApplicationInsights({
      config: {
        connectionString: connectionString,
        enableAutoRouteTracking: true, // Track route changes in SPA
        enableRequestHeaderTracking: true,
        enableResponseHeaderTracking: true,
        disableExceptionTracking: false,
        disableTelemetry: false,
        enableUnhandledPromiseRejectionTracking: true
      }
    })

    this.appInsights.loadAppInsights()
    this.appInsights.trackPageView() // Track initial page view
  }

  public trackEvent(name: string, properties?: Record<string, string | number | boolean>): void {
    if (this.appInsights) {
      this.appInsights.trackEvent({ name, properties })
    }
  }

  public trackException(exception: Error, properties?: Record<string, string | number | boolean>): void {
    if (this.appInsights) {
      this.appInsights.trackException({ exception, properties })
    }
  }

  public trackTrace(message: string, properties?: Record<string, string | number | boolean>): void {
    if (this.appInsights) {
      this.appInsights.trackTrace({ message, properties })
    }
  }

  public setUserId(userId: string): void {
    if (this.appInsights) {
      this.appInsights.setAuthenticatedUserContext(userId)
    }
  }

  public clearUserId(): void {
    if (this.appInsights) {
      this.appInsights.clearAuthenticatedUserContext()
    }
  }
}

// Create singleton instance
export const appInsights = new AppInsightsService()