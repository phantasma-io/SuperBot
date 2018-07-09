using System;
using MiddlemanLayer;

/// <summary>
/// TODO
/// implement sub groups in reactions. They will provide chaining of reactions, each sub group will
/// have one reaction chosen. 
/// 
/// Add a field for reaction delay.async Possibly as subgroup modifiers?
/// </summary>

namespace DataLayer{
    public partial class ReactionGroup {
        static Random rand = new Random();
        public ReactionOutputMessage SelectReaction()
        {
            var chosenIndex = rand.Next(0,reactions.Count - 1);
            var reaction = reactions[chosenIndex];

            return new ReactionOutputMessage(reaction.reactionType, reaction.value);
        }
    }
}