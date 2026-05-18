EXTERNAL addFriend(friend_name)
EXTERNAL changeHappiness(name, delta)

VAR player_name = "Player"
VAR player_happiness = 0
VAR player_friends_count = 0

VAR npc_name = "Npc"
VAR npc_happiness = 0


-> friendly_sphere

=== lonely_sphere ===
#speaker: {npc_name}
I really feel lonely.

I'm just a ball frozen in infinity empty world created by the developer.
+ [Warm greeting]
    #speaker: {player_name}
    "Hey {npc_name}! Great to see you!"
        ~ changeHappiness(npc_name, 2)
        ~ changeHappiness(player_name, 1)
        # anim: sphere:color_green
        Oh! {player_name}! What a pleasant surprise!
        -> check_help
    
+ [Neutral greeting] 
    #speaker: {player_name}
    "Hello {npc_name}."
        ~ changeHappiness(npc_name, 1)
        Oh... hello {player_name}.
        -> check_help
    
+ [Cold greeting] 
    #speaker: {player_name}
    [ Just nod silently ]
        # anim: sphere:color_fade
        ... (uncomfortable silence)
        -> check_help
    
-> END

=== check_help ===
    You want to help me? 
#anim: sphere:question
{ npc_happiness > 4: I'm really glad to see you. | You seem fine to be my friend }


    + [Yes!]
        #speaker: {player_name}
        Yes I agree!
        #speaker: {npc_name}
        # anim: sphere:color_green
        Okay! Great!
        ~ changeHappiness(npc_name, 1)
        ~ changeHappiness(player_name, 1)
        ~ addFriend(npc_name)
        -> END
        
    + {npc_happiness > 7} [Hug you] 
        # anim: sphere:color_green
        ~ addFriend(npc_name)
        ~ changeHappiness(npc_name, 1)
        ~ changeHappiness(player_name, 1)
        #speaker: {npc_name}
        Aww, thanks!
        -> END
    
    
    + [No!]
        # anim:sphere:color_fade
        #speaker: {npc_name}
        Sorry to bother
        ~ changeHappiness(npc_name, -1)
        -> END
        
        
-> END

===friendly_sphere===

#speaker: {npc_name}
- Hi, {player_name}
How are you?
#anim: sphere:question
{player_friends_count > 0: 
    Wow you have already {player_friends_count} {player_friends_count > 1: friends | friend}! I wanna be your friend too!
   
    - else:
    You dont have much friends are you?
    Well at least you can have me as friend
    }
    
    + [Agree]
    #speaker: {player_name}
    I agree!
    ~ addFriend(npc_name)
    
     #anim: sphere:color_green
     #speaker: {npc_name}
     Great!
    -> END
    
    + [Refuse]
    #speaker: {player_name}
    Sorry, but no.
    # anim:sphere:color_fade
    #speaker: {npc_name}
    Oh, thats okay
    -> END

-> END

// Placeholder to avoid Ink errors
=== function addFriend(friend_name) ===
// You can make it return a default value or just leave it empty
~ return

=== function changeHappiness(name, delta) ===
// You can make it return a default value or just leave it empty
~ return