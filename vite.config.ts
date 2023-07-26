import { defineConfig } from 'vite'
import fs from 'fs'

const settingPath = './Chat/Client/wwwroot/appsettings.json'

export default defineConfig({
    root: './Chat/Client/web',
    build: {
        outDir: '../wwwroot',
        rollupOptions: {
            plugins: [{
                name: 'generate-appsettings',
                closeBundle() {
                    // Generate appsettings.json
                    // Read Azure AD B2C client settings from environment variables
                    const config = {
                        AzureAdB2C: {
                            Authority: process.env["AZADB2C_AUTHORITY"],
                            ClientId: process.env["AZADB2C_CLIENTID"],
                            ValidateAuthority: false
                        },
                        Mock: false,
                        Scope_Uri: process.env["SCOPE_URI"],
                    }
                    if (process.env.NODE_ENV === 'mock') {
                        config.Mock = true
                    }
                    fs.writeFileSync(settingPath, JSON.stringify(config))
                },
            }],
            external: ['public'],
            output: {
                entryFileNames: 'js/[name].js',
                chunkFileNames: 'js/[name].js',
                assetFileNames: '[ext]/[name].[ext]',
            }
        }
    }
})


