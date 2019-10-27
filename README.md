<h4 align="center">
  <img alt="common readme" src="https://i.imgur.com/5zpfhLm.png" width="306.3" height="350">
</h4>
<h4 align="center">
  The really friendly discord bot that helps you make channel pairs
</h4>

Gabby Picture by [DashieSparkle](https://www.deviantart.com/dashiesparkle)

---

## What does she do?
Gabby lets Discord server owners and managers create channel pairs. It also tracks who is connected to those channels and will show/hide the text channel in the pair when they connect or disconnect repspectively.

## What is a channel pair?
There might be a better name for it, but a channel pair is a text and a voice channel in discord that are directly linked.

In TeamSpeak each voice channel had a text chat attached to it. Only those connected to the channel could see that chat which made things nice and clean. Discord as of yet hasn't added the same behaviour so a lot of Discord servers emulate this feature by making a pair of channels that sit next to each ohter in the tree like below.

![Picture of Channel Pair](https://i.imgur.com/OLV4CcF.png)

I feel like this is messy (especially with more than one) and doesnt give the same effect as people outside of the voice channel can continue to type in the text chat.

## What can Gabby do to help?
Gabby helps by setting up these channel pairs and monitoring them so that when a user joins a voice channel with a connected text channel connected connected to it, then it will appear for them and dissapear when they disconnect.

![Demonstration of Gabby](https://i.imgur.com/585lsai.gif)

She does this by using a role that is named like the voice channel. The channel permissions on the text channel are set so that everyone cannot see it, but the name role can. Gabby then assigns and removes this role from users as they connect and disconnect respectively.

Gabby also features some commands that lets server owners set up these channel pairs with sease and remove them just as quickly.

## How can I use her on my server?
Currently I'm still perfecting functionality and making her more reliable. Once this is done I may consider hosting her publicly.

If you wish to use her now then feel free to clone this repo and use `dotnet run` to build and run the project.
Gabby is built on .NET Core 2.1 using the Discord.Net API.
**NOTE: Make sure to put your token in the `_config.yml` file**
