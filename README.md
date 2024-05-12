# TONWallet
## Crossplatform command-line interface TON wallet with seed support.

TONWallet is a wallet designed to run in a console window. It is oriented towards minimalism and security. TONWallet turns your TON wallet into a vault and a persistent impenetrable fund storage.

- No Web3 integrations
- No bloat, just the basic features you need to control your funds
- The wallet for **true** TON enthusiasts
- 🌟 You are entirely in control 🌟

## Features

* [x] Masterkey-enforced AES256 encryption of wallets
* [x] Generating and importing TON wallets using: mnemonic phrase (✅), seed (✅), and hardware (❌)
* [x] Wallet deploy
* [x] Balance and account lookup
* [ ] Transaction lookup: ingoing (⚠) and outgoing (❌)
* [x] Anti-fake transaction classifier
* [x] Viewing and sending TON
* [ ] Viewing and sending NFTs
* [ ] Viewing and sending Jettons
* [ ] Staking support
* [ ] Portable version
* [ ] Custom blockchain messages

The wallet is in active development. Keep in mind there currently are no plans to add any integration with Web3 applications.

## Installation

To run TONWallet, you will need to have **.NET 8** installed.

You can build TONWallet from **source code (preferred for safety)** or get it through Releases. To build TONWallet from source code, download the repository, then navigate to TONWallet directory and build it:
```bash
git clone https://github.com/kolya5544/TONWallet.git
cd TONWallet
dotnet build
```

Once done, you should receive binary files located in `/bin/Debug/net8.0`. To run TONWallet, navigate to the directory and run the .dll file:
```bash
dotnet TONWallet.dll
```

## Contributions

I do not accept any big contributions or major code changes. Feel free to use GitHub Issues to report bugs and request features to be implemented.

## License

MIT