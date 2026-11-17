// So para rodar no navegador: o expo-sqlite na web carrega um arquivo .wasm.
// (No navegador ele tambem exige os cabecalhos COOP/COEP; no celular, com o Expo Go, nada disso e usado.)
const { getDefaultConfig } = require('expo/metro-config');

const config = getDefaultConfig(__dirname);

config.resolver.assetExts.push('wasm');

module.exports = config;
