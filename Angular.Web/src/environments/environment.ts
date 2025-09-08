export const environment = {
    production: false,
    auth: {
        issuer: 'https://localhost:7108', // Duende base URL
        clientId: 'angular-spa', // MUST exist in Duende clients
        redirectUri: 'http://localhost:4200/auth/callback',
        postLogoutRedirectUri: 'http://localhost:4200/',
        scopes: 'openid profile payments.read accounting.read',
    },
    apis: {
        payments: 'https://localhost:7024', // e.g., Payments.Api
        accounting: 'https://localhost:7095' // e.g., Accounting.Api
    }
};