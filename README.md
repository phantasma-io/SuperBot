# Phantasma Bot 2.0

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
    "triggerType": TriggerTypeEnum,
    "data": object,
    "modifier": int,
    "subgroup": "string",
    "onFailMsg": "string"
}
```

TriggerTypes and TriggerModifiers, along with their meanings, are defined in TriggerEnums.cs

Trigger Type | Meaning
---------|----------
 "Text" | A text input trigger
 "DictionaryVariable" | Dictionary User-Variable trigger
 "Image" | An image input trigger

### "data" property structure

For each trigger type, there is a certain expected object type for the "data" property.

Trigger Type | "data" property structure
---------|----------
"Text" | ```"data": "regex expression string"```
 "DictionaryVariable" | ```"data": {"variable": "variable name string", "value": null/"variable value string"}```
 "Image" | ```"data": {"minSize": null/float (megabytes), "maxSize": null/float (megabytes)```

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
 0 | Checks if the data sent by the user is an image within a specified size (up to a maximum of 20MB due to telegram limitations)

### Trigger Groups and Sub-groups

Each trigger has a group and a sub-group.

```json
{
    "trigger group name":{
        "privateTrigger": true,
        "triggers": [
            {
                "triggerType": 0,
                "value": "abc",
                "modifier": 0,
                "subgroup": ""
            },
            {
                "triggerType": 0,
                "value": "zxy",
                "modifier": 0,
                "subgroup": "sub"
            },
            {
                "triggerType": 0,
                "value": "g",
                "modifier": 0,
                "subgroup": "sub"
            }
        ]
    }
}
```

Here we have a trigger on _Group_ "trigger group name" with 2 _Sub-groups_, the default one and _Sub-group_ "sub". _Sub-group_ "sub" consists of 2 triggers.

**A _TriggerGroup_ is triggered when at least one _Trigger_ of each _TriggerSubgroup_ is triggered.**

If we were to name each of the above presented Triggers as A, B and C respectively, Trigger _Group_ would be evaluated according to the following expression:
`(A) && (B || C)`

Each Trigger _Group_ has a property called `privateTrigger` that will define what kind of chat can trigger this.

#### Usage Details

The _TriggerGroup_ order on the file matters! The _TriggerGroup_ list is evaluated from first to last, only the first _TriggerGroup_ that returns true will be triggered.

The _TriggerSubgroup_ order should also be taken into account. For optimal performance, they should be ordered from least likely to most likely. This is because the _TriggerSubgroup_ evaluation stops on the first one that returns false, as the _TriggerGroup_ requires each _TriggerSubgroup_ to be true.

### Reactions

A _Reaction_ is defined as follows:

```json
{
    "reactionType": ReactionTypeEnum,
    "value": "string"
}
```

_ReactionTypes_ are defined in ReactionEnums.cs

Reaction Type | Meaning
---------|----------
 0 | A simple text reply reaction
 1 | Make changes to the singleVariables
 2 | Make changes to the dictionaryVariables

#### Reaction Groups and Sub-groups

Each Reaction has a group, and will have a sub-group in the future

```json
{
    "behaviourKey label":{
        "reactions": [
            {
                "reactionType": 0,
                "value": "Shut up asshole :("
            },
            {
                "reactionType": 1,
                "value": {
                    "variable": "bot state",
                    "value": "Idle"
                }
            },
            {
                "reactionType": 2,
                "value": {
                    "variable":"whitelist",
                    "value":"Accepted"
                }
            }
        ]
    }
}
```

*************************
ESCREVE A PORRA DA CENA SOBRE A AUSENCIA DE KEY NO REACTION TYPE 2
partimos destes pressupostos:
* que as keys dos dicionarios vao ser sempre user id's
* que nunca vamos querer aplicar uma reaction a um user diferente do que lhe deu trigger

por isso, nós não passamos a key para um reaction type 2, porque vamos simplesmente usar como key o campo de userId que vem da reaction, que foi o que causou o trigger.

*****************************

If a _ReactionGroup_ is called, each _ReactionSubgroup_ is executed in order from lowest subgroup index to highest, with each subgroup having to pick through some method just one of its contained reactions. For this example it would mean that the bot would either react with reactions 1 and 3, or reactions 2 and 3.

A delay can be configured, measured in seconds, for each reaction

### User Variables

```json
{
    "variable name": {
        "saveMethod": -1,
        "isDictionary": false,
        "defaultValue": ""
    }
}
```

saveMethod | Meaning
---------|----------
 -1 | Never save to file
 0 | Save to file immediately after change
 >0 | Save every x minutes

#### Dictionary User Variables

 If variable is a dictionary, it gets added to a Dictionary of Dictionaries, representing a list for variables you want to store per user, i.e. each user's kyc state.

In case these are saved to file, the file name will be _variable name_.

#### Global User Variables
