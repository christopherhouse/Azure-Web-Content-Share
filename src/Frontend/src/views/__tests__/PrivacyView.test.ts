import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createVuetify } from 'vuetify'
import PrivacyView from '../PrivacyView.vue'

const vuetify = createVuetify()

describe('PrivacyView', () => {
  it('renders the privacy page with expected content', () => {
    const wrapper = mount(PrivacyView, {
      global: {
        plugins: [vuetify],
      },
    })

    // Check that the main privacy heading is present
    expect(wrapper.text()).toContain('Privacy Policy')
    
    // Check for key privacy content sections
    expect(wrapper.text()).toContain('Data Protection')
    expect(wrapper.text()).toContain('Authentication')
    expect(wrapper.text()).toContain('Encryption')
    expect(wrapper.text()).toContain('Azure')
  })

  it('contains security and protection information', () => {
    const wrapper = mount(PrivacyView, {
      global: {
        plugins: [vuetify],
      },
    })

    // Check for security-related content
    expect(wrapper.text()).toContain('RBAC')
    expect(wrapper.text()).toContain('encryption')
    expect(wrapper.text()).toContain('secure')
  })

  it('has an engaging design with emojis', () => {
    const wrapper = mount(PrivacyView, {
      global: {
        plugins: [vuetify],
      },
    })

    // Check for presence of emojis in content
    const text = wrapper.text()
    const emojiRegex = /[\u{1F600}-\u{1F64F}]|[\u{1F300}-\u{1F5FF}]|[\u{1F680}-\u{1F6FF}]|[\u{1F1E0}-\u{1F1FF}]|[\u{2600}-\u{26FF}]|[\u{2700}-\u{27BF}]/u
    expect(emojiRegex.test(text)).toBe(true)
  })
})