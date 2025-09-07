export const environment = {
    production: false,
    auth: {
        issuer: 'https://localhost:5001', // Duende base URL
        clientId: 'angular-spa', // MUST exist in Duende clients
        redirectUri: window.location.origin + '/auth/callback',
        postLogoutRedirectUri: window.location.origin + '/',
        scopes: 'openid profile payments.read accounting.read',
    },
    apis: {
        payments: 'https://localhost:5003', // e.g., Payments.Api
        accounting: 'https://localhost:5004' // e.g., Accounting.Api
    }
};