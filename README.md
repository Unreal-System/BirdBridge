# BirdBridge
Runtime: .NET Core 7

Download Twitter Photo/Video Bot

Publish To Docker:

docker build -t tg_twitter_bot .

docker image save tg_twitter_bot | xz -z -e -9 -T 0 > twitterbot.tar.xz
