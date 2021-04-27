## Events

### ChatMessageEvent
```c#
public Dictionary<string, int> Badges;     // Badges associated to the user who sent the message.
public string Color;                       // A string representing a color value associated with message.
public string DisplayName;                 // The display name of the user who sent the message.
public List<Emote> Emotes;                 // List of emotes in the message.
public HashSet<string> Flags;              // Set of flags that can be defined by other implementations. An example is the default Twitch implementation sets a flag called `IsMe` when the message is sent with the `/me` command.
public bool Mod;                           // Boolean value indicating if the message was sent by a channel moderator.
public bool Subscriber;                    // Boolean value indicating if the message was sent by a subscriber.
public bool Broadcaster;                   // Boolean value indicating if the message was sent by the broadcaster.
public long Timestamp;                     // Unix timestamp in seconds using UTC.
public string UserType;                    // User type.
public string Username;                    // The username of the user who sent the message.
public string Channel;                     // The channel where the message was sent.
public string MessageContent;              // The actual message.
```

### ChatMessageWithRewardEvent
```c#
public Dictionary<string, int> Badges;     // Badges associated to the user who sent the message.
public string Color;                       // A string representing a color value associated with message.
public string DisplayName;                 // The display name of the user who sent the message.
public List<Emote> Emotes;                 // List of emotes in the message.
public HashSet<string> Flags;              // Set of flags that can be defined by other implementations. An example is the default Twitch implementation sets a flag called `IsMe` when the message is sent with the `/me` command.
public bool Mod;                           // Boolean value indicating if the message was sent by a channel moderator.
public bool Subscriber;                    // Boolean value indicating if the message was sent by a subscriber.
public bool Broadcaster;                   // Boolean value indicating if the message was sent by the broadcaster.
public long Timestamp;                     // Unix timestamp in seconds using UTC.
public string UserType;                    // User type.
public string Username;                    // The username of the user who sent the message.
public string Channel;                     // The channel where the message was sent.
public string MessageContent;              // The actual message.
public string RewardId;                    // The ID of the reward associated with the message.
```

### HostingStartedEvent
```c#
public string TargetChannel;               // The channel being hosted.
public string HostingChannel;              // The channel that's hosting.
public int Viewers;                        // The amount of viewers from the hosting channel.
public long Timestamp;                     // Unix timestamp in seconds using UTC.
```

### HostingStoppedEvent
```c#
public string HostingChannel;              // The channel that was hosting.
public int Viewers;                        // The amount of viewers from the hosting channel.
public long Timestamp;                     // Unix timestamp in seconds using UTC.
```

### HostNotificationEvent
```c#
public string TargetChannel;               // The channel being hosted.
public string HostingChannel;              // The channel that's hosting.
public bool IsAutoHost;                    // Boolean value indicating if the host was started automatically.
public int Viewers;                        // The amount of viewers from the hosting channel.
public long Timestamp;                     // Unix timestamp in seconds using UTC.
```

### NewSubscriptionEvent
```c#
public Dictionary<string, int> Badges;     // Badges associated to the user.
public string Color;                       // A string representing a color value associated with the message or user.
public string DisplayName;                 // The display name of the user.
public List<Emote> Emotes;                 // List of emotes in the message.
public HashSet<string> Flags;              // Set of flags that can be defined by other implementations.
public bool Mod;                           // Boolean value indicating if the user is a channel moderator.
public bool Subscriber;                    // Boolean value indicating if the user is a subscriber.
public long Timestamp;                     // Unix timestamp in seconds using UTC.
public string UserType;                    // User type.
public string Username;                    // The username of the user who subscribed to the channel.
public string Channel;                     // The channel where the user subscribed.
public string MessageContent;              // The message supplied by the user.
public SubscriptionPlan SubscriptionPlan;  // Enum identifying the subscription plan.
public string SubscriptionPlanName;        // The name of the subscription plan.
public int CumulativeMonths;               // The total amount of months the user has been subscribed.
public bool ShouldShareStreak;             // Boolean value indicating whether the subscription streak should be shared.
public int StreakMonths;                   // The total amount of months in a row the user has been subscribed.
public string SystemMessage;               // A subscription message supplied by the event implementation.
```

### ReSubscriptionEvent
```c#
public Dictionary<string, int> Badges;     // Badges associated to the user.
public string Color;                       // A string representing a color value associated with the message or user.
public string DisplayName;                 // The display name of the user.
public List<Emote> Emotes;                 // List of emotes in the message.
public HashSet<string> Flags;              // Set of flags that can be defined by other implementations.
public bool Mod;                           // Boolean value indicating if the user is a channel moderator.
public bool Subscriber;                    // Boolean value indicating if the user is a subscriber.
public long Timestamp;                     // Unix timestamp in seconds using UTC.
public string UserType;                    // User type.
public string Username;                    // The username of the user who subscribed to the channel.
public string Channel;                     // The channel where the user subscribed.
public string MessageContent;              // The message supplied by the user.
public SubscriptionPlan SubscriptionPlan;  // Enum identifying the subscription plan.
public string SubscriptionPlanName;        // The name of the subscription plan.
public int CumulativeMonths;               // The total amount of months the user has been subscribed.
public bool ShouldShareStreak;             // Boolean value indicating whether the subscription streak should be shared.
public int StreakMonths;                   // The total amount of months in a row the user has been subscribed.
public string SystemMessage;               // A subscription message supplied by the event implementation.
```