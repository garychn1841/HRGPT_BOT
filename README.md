# SKFH_HRGPT_BOT

Bot Framework v4 SKFH_HRGPT_BOT sample.

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to create a simple bot that accepts input from the user and echoes it back.

## SKFH_HRGPT_BOT Software Architecture

![Software Architecture](https://github.com/garychn1841/SKFH_HRGPT_BOT/blob/master/pic/Software%20Architecture%201.png)

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) version 6.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## To try this sample

- In a terminal, navigate to `SKFH_HRGPT_BOT`

    ```bash
    # change into project folder
    cd SKFH_HRGPT_BOT
    ```

- Run the bot from a terminal or from Visual Studio.

  ```bash
  # run the bot
  dotnet run
  ```


## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.9.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

`Notice: If you want to test your bot with redis, it better run a lacal redis container by docker on your pc`    

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.


