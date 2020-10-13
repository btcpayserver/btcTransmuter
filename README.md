# BtcTransmuter Docs

## Introduction - What is BtcTransmuter?

BtcTransmuter is a free, open-source & self-hosted tool that allows you to configure actions that execute automatically upon specified conditions. Its primary focus is the integration of cryptocurrency services to help users manage their funds and business workflow.

## How does it work?
There are 3 main components in BtcTransmuter - External Services, Recipes and Extensions. 

* External services integrated with third parties, such as a BTCPayServer instance or a Cryptocurrency Exchange.
* Recipes is a set of instructions created by the user: What to execute and when. The execution part is called a Recipe Action while the condition is called a Recipe Trigger. An action could be `Send an Email using the SMTP External Service` and the trigger being `If a new invoice was created on my BtcPay External Service`
* Extensions provide a way to add more external service types, actions and triggers without needing to modify the original code.

## Can I use this in production
This project is still in early development and most likely has some bugs and quirks that need to be ironed out. The project is considered to be in ALPHA state, meaning not all desired functionality has been implemented and there can be breaking changes.

## How do I deploy?

If you set up BTCPay using any [dockerized install method](https://docs.btcpayserver.org/Docker/), you can enable BTC Transmuter easily.

First, [SSH](https://github.com/JeffVandrewJr/patron/blob/master/SSH.md) into your server.
Then run the following commands:
```bash
sudo su -
cd btcpayserver-docker
export BTCPAYGEN_ADDITIONAL_FRAGMENTS="$BTCPAYGEN_ADDITIONAL_FRAGMENTS;opt-add-btctransmuter"

. ./btcpay-setup.sh -i
```

If you have trouble starting your server after running the setup command, verify you have [added the environment variable](https://docs.btcpayserver.org/FAQ/FAQ-Deployment/#how-can-i-modify-or-deactivate-environment-variables) correctly to enable Transmuter.

Once completed, your Transmuter will be available at your configured BTCPay website under `/btctransmuter/`, where the first account to register becomes the Transmuter admin. Open that link in your browser and you’ll see the homepage.

## What extensions are provided?

You can see a list of built-in extensions once you log into your transmuter application under `/extensions`.

Currently there are 12 extensions that come bundled with the main application:
* Tor - Provides TOR access to your Transmuter app
* Presets - Provides one page wizards for quick setups of common scenarios
* BtcPayServer - Provides you a way to interact with BTCPayServer stores
* Email - Provides you with a way to check a POP3 for incoming emails and a way to send Email via SMTP
* Exchange - Provides you a way to connect to a variety of exchanges and place orders on them
* Lightning - Provides you interoperability with lightning nodes to be able to do basic channel and invoice management 
* NBXplorer - Provides you ways to detect detect incoming funds, send funds, and generate addresses.
* Operators - Lets you create conditional workflows
* Recipe - Lets you manage existing or create recipes with your workflows
* Timer - Run X every Y
* WebHook - Trigger Recipes with an http call or execute external web hooks with a recipe

## What preset recipes are provided?
Currently there are three preset recipes that you can enable easily. In addition to the list below, you can create your own recipes for automating other tasks.
1. On-chain Forwarder - Forward funds from a wallet elsewhere
2. [BTCPay Email Receipts](docs/EmailReceiptsPreset.md) - Send an email when a BTCPay invoice gets paid
3. Fiat Conversion - Convert incoming money to fiat on an exchange by market selling when BTCPay invoice statuses change.

## How do I write an extension?

An extension must be its own .net core class library project that references `BtcTransmuter.Abstractions`
There must be a class implementing `BtcTransmuterExtension` in this library. This file bootstraps the plugin name and adds all the services to system on startup.

### Adding a Trigger

* Implement `BaseTrigger` This class will be used to transport the event data
* Implement `BaseTriggerHandler` This handles:
  * Describing the trigger to the main system. 
  * Telling the main UI how to create/edit a recipe trigger using it.
  * The logic needed to see if a trigger is...triggered.
  
  You will see that `BaseTriggerHandler` has 2 generic arguments, a `Data` and `Parameters`
   `Data` is the trigger that that was detected, while `Parameters` is the data from a Recipe to see if it triggers its actions. 
* Implement a Partial View (named as the same value as the property `ViewPartial` in the `BaseTriggerHandler` implementation)
  This handles displaying the trigger settings in a recipe
* Implement a Controller that allows a user to create/edit the necessary data to set the trigger on a recipe.

### Adding an Action
* Implement `BaseActionHandler` This handles:
  * Describing the action to the main system. 
  * Telling the main UI how to create/edit a recipe action using it.
  * The logic needed to execute the action
  
  You will see that `BaseActionHandler` has a generic argument, `Data`
   `Data` is the type of the action that holds the payload needed to exeucte the action through the instructions of recipe action
* Implement a Partial View (named as the same value as the property `ViewPartial` in the `BaseActionHandler` implementation)
  This handles displaying the action settings in a recipe
* Implement a Controller that allows a user to create/edit the necessary data to set the action on a recipe.


### Adding an External Service
* Implement `BaseExternalService` & `IExternalServiceDescriptor`
* Implement a Partial View (named as the same value as the property `ViewPartial` in the `IExternalServiceDescriptor` implementation)
* Implement a Controller that allows a user to create/edit the necessary data for an external service.
