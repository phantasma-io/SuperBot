**Phantasma Bot 2.0**

Structured approach with separation of communications layer (discord, telegram, etc.) and data layer (json with settings and whatnot), with a middleman layer implementing the façade pattern between them.

The general architecture is based on a trigger-reaction workflow. In case a trigger is triggered, the reaction associated with it will be used to determine the output action of the bot.

To store user related data, the concept of "user dictionary variables" was developed. A variable of these simply consists of two columns, the first stores user IDs and the second stores a string associated with each of those IDs. Example: "whitelist state" user dictionary variable contains the user id's of the users that attempted whitelist, and holds the state of their whitelist process on the second column.

A powerful and flexible framework was developed for this bot to allow configuration capabilities and tailoring to the user's needs without having to mess with code, instead loading all the needed configurations from JSON (for triggers and reactions) and a MySQL database (for dictionary variables).

# Data Layer

Responsible for interacting with JSON files and abstracting their structure details to the MiddlemanLayer

## Settings files

**This is planned to change into regular JSON DBO parsing like in the rest of the cases**
They are read into a dynamic type variable, if you mess with the existing names on json without updating the code it will crash.
We did this because these are meant to be changed only sporadically (once on first setup for a service purchaser,and again with each up/downgrade of the package), and only by us developers, not by customers, so it's not as critical.

## Triggers

A _Trigger_ is defined as follows:

```json
{
    "triggerType": "TriggerTypeEnum",
    "data": object, (based on the corresponding modifier enum for the given trigger type)
    "modifier": int,
    "subgroup": null/string,
    "onFailMsg": null/string
}
```

TriggerTypes and TriggerModifiers, along with their meanings, are defined in TriggerEnums.cs

Trigger Type | Meaning
---------|----------
 "Text" | Depends on the text message sent by the user
 "DictionaryVariable" | Depends on a specified dictionary variable associated with the user that sent the message
 "Image" | Depends on the image sent by the user

### Triggers' "data" property structure

For each trigger type, there is a certain expected object type for the "data" property.

Trigger Type | "data" property structure
---------|----------
"Text" | ```"data": "regex expression string"```
"DictionaryVariable" | ```"data": {"variable": "string", "value": null/"string"}```
"Image" | ```"data": {"minSize": null/float, "maxSize": null/float```

### Trigger Modifiers

These will define how a specific trigger type will be parsed, and are defined as follows:

Text Trigger Modifier | Meaning
---------|----------
 0 | Checks if the defined regex has any hits on the user's message

 Dictionary Variable Trigger Modifier | Meaning
---------|----------
 0 | Checks if the given variable name has the given value for the user that sent the message.
 1 | Checks if the given variable name has a row for the user that sent the message

 Image Trigger Modifier | Meaning
---------|----------
 0 | Checks if the data sent by the user is an image within a specified size (note to users: telegram only supports images up to 20MB)

### Trigger Groups and Sub-groups

Each trigger has a group and a sub-group.

```json
{
    "trigger group name":{
        "privateTrigger": true,
        "triggers": [
            {
                "triggerType": "TriggerTypeEnum",
                "modifier": int,
                "data": object,
                "subgroup": null/string,
                "onFailMsg": null/string
            },
            ...
        ]
    }
}
```

Here we have a trigger on _Group_ "trigger group name" with 2 _Sub-groups_, a random one and _Sub-group_ "sub". _Sub-group_ "sub" consists of 2 triggers.

**A _TriggerGroup_ is triggered when at least one _Trigger_ of each _TriggerSubgroup_ is triggered.**

If we were to name each of the above presented Triggers as A, B and C respectively, Trigger _Group_ would be evaluated according to the following expression:
`(A) && (B || C)`

When "subgroup" is null, a random GUID is assigned as the subgroup name, turning the default behaviour of triggers as all being required to activate their TriggerGroup.

Each _TriggerGroup_ has a property called `privateTrigger` that defines what kind of chat can trigger this. If this property is true, then only private PM's to the bot can trigger this _TriggerGroup_.

The _TriggerGroup_ order on the file matters! The _TriggerGroup_ list is evaluated from first to last, only the first _TriggerGroup_ that returns true will be triggered.

The _TriggerSubgroup_ order should also be taken into account. For optimal performance, they should be ordered from least likely to most likely. This is because the _TriggerSubgroup_ evaluation stops on the first one that returns false, as the _TriggerGroup_ requires each _TriggerSubgroup_ to be true.

### "onFailMsg" Property

This is a powerful feature, but **it must be used with care as it can easily introduce unexpected outputs if used incorrectly.**

The purpose of this feature is to enable simple text feedback for different fail scenarios inside a _TriggerGroup_ without having to define several trigger-reaction pairs to give feedback on why the trigger failed.
For example, during a whitelist procedure where the user is trying to input an email, we can just define an _onFailMsg_ to warn him in case of bad format, instead of having to define a _TriggerGroup_ for the negation of the email format regex and a corresponding reaction to say the same message to the user.

These are the properties of the current implementation of this feature:

* Any trigger can have an "onFailMsg" property defined;
* When a subgroup evaluation fails, the "onFailMsg" for the last trigger on that subgroup will be selected;
* If the selected "onFailMsg" is not null and not empty, **all _TriggerGroup_ evaluation stops as if one was evaluated as true**;
* The selected "onFailMsg" is then sent to the user.

The dangers in using this feature are these:

* If an "onFailMsg" is defined on the first subgroup of any _TriggerGroup_ T, then any _TriggerGroup_ after T will effectively be eclipsed by it: if T is true, the corresponding reaction is used; if T is false, the "onFailMsg" is used as a reaction. This prevents any subsequent _TriggerGroup_ from ever being evaluated at all.

* If an "onFailMsg" is defined on any non-first subgroup S of a _TriggerGroup_ T, but the previous subgroups don't provide a fence for S where it is only evaluated when T is definitely the _TriggerGroup_ in question, then it is possible that subsequent _TriggerGroup_ eclipsing may occur but only occasionally, making debugging harder.

An example follows:

```json
"kyc wallet": {
    "privateTrigger": true,
    "triggers": [
        {
            "triggerType": "DictionaryVariable",
            "data": {
                "variable": "kyc status",
                "value": "need wallet"
            },
            "modifier": 0,
            "subgroup": "status check"
        },
        {
            "triggerType": "Text",
            "data": "\\S*",
            "modifier": 0,
            "subgroup": "format check",

            "onFailMsg": "Wrong wallet address you moron"
        }
    ]
},
```

Here we can see subgroup "format check" being fenced by "status check", where we are sure that when "kyc status" for a given user is "need wallet", the only answer we are expecting from this user is their wallet. In case it fails the regex, we don't care about eclipsing subsequent trigger groups as we know this is the only relevant one for the current state of the conversation.

If "status check" subgroup didn't exist in this trigger, then the "onFailMsg" of "format check" would always be sent to the user if all trigger groups before "kyc wallet" would evaluate as false, eclipsing the subsequent ones.

## Reactions

A _Reaction_ is defined as follows:

```json
{
    "reactionType": "ReactionTypeEnum",
    "modifier": int,
    "data": object,
    "subgroup": null/string
}
```

_ReactionTypes_ are defined in ReactionEnums.cs

Reaction Type | Meaning
---------|----------
 "Text" | A simple text reply reaction
 "DictionaryVariable" | Reactions dealing with dictionaryVariables
 "IO" | Reactions dealing with file creation/storage

### Reactions' "data" property structure

For each reaction type, there is a certain expected object type for the "data" property.

Reaction Type | "data" property structure
---------|----------
"Text" | ```"data": "string"```
"DictionaryVariable" | ```"data": {"variable": "string", "value": null/"string"}```
"IO" | ```"data": {"minSize": null/float, "maxSize": null/float```

### Reaction Modifiers

These will define how a specific reaction type will be parsed, and are defined as follows:

Text Reaction Modifier | Meaning
---------|----------
 0 | Replies a predefined text message to a predefined chat id contained in a _SingleVariable_ **to be implemented, currently only replies to the same chat**

 Dictionary Variable Trigger Modifier | Meaning
---------|----------
 0 | Checks if the given variable name has the given value for the user that sent the message
 1 | Checks if the given variable name has a row for the user that sent the message

 Image Trigger Modifier | Meaning
---------|----------
 0 | Checks if the data sent by the user is an image within a specified size in MB

### Reaction Groups and Sub-groups

Each Reaction has a group and a sub-group.

```json
{
    "ReactionGroup name":{
        "reactions": [
            {
                "reactionType": "Text",
                "modifier": 0,
                "data": "Shut up asshole :(",
                "subgroup": "response"
            },
            {
                "reactionType": "Text",
                "modifier": 0,
                "data": "I love you",
                "subgroup": "response"
            },
            {
                "reactionType": "DictionaryVariable",
                "modifier": 0,
                "data": {
                    "variable":"whitelist",
                    "value":"Accepted"
                },
                "subgroup": null
            }
        ]
    }
}
```

If a _ReactionGroup_ is called, each _ReactionSubgroup_ is executed in the same order they were defined in JSON. If a given subgroup has more than one reaction, one of them is selected randomly. In the above example it would mean that the bot would either react with reactions 1 and 3, or reactions 2 and 3.

When "subgroup" is null, a random GUID is assigned as the subgroup name, turning reactions' default behaviour as all being activated when their reaction group is chosen.

## User Variables

```json
{
    "variables": [
        {
            "variableName": "name",
            "saveToDisk": bool,
            "isDictionary": bool,
            "value": null/"value"
        },
        ...
    ]
}
```

If saveToDisk is `true`, then we will save every change to that variable into the MySQL database, and load it back up everytime the bot restarts.

### Dictionary User Variables

If variable is a dictionary, it gets added to a Dictionary of Dictionaries, representing a list for variables you want to store per user, i.e. each user's kyc state.

In case these are saved to file, the file name will be _variable name_.

*************************
ESCREVE A PORRA DA CENA SOBRE A AUSENCIA DE KEY NO REACTION TYPE 2
partimos destes pressupostos:
* que as keys dos dicionarios vao ser sempre user id's
* que nunca vamos querer aplicar uma reaction a um user diferente do que lhe deu trigger

por isso, nós não passamos a key para um reaction type 2, porque vamos simplesmente usar como key o campo de userId que vem da reaction, que foi o que causou o trigger.

*****************************

### Single User Variables

If saveToDisk is true, not only will it save any changes of the variable's value to the database, but everytime the bot restarts it will also attempt to overwrite the start value defined in JSON (if it is defined) with any value stored on the database!