const mongoose = require("mongoose");

class MatchController{
    async getAllMatches(req, res){
        try {
            const Match = mongoose.model('Match');
            let matches = await Match.find();
            res.status(200).json(matches);
        } catch (error) {
            res.status(500).json({ error: error.message });
        }
    }

    async postMatch(req, res){
        try {
            const Match = mongoose.model('Match');
            const User = mongoose.model('User');
            const { winnerId, loserId, loserGoals } = req.body;

            // Create new match record
            const newMatch = new Match({ winnerId, loserId, loserGoals });
            await newMatch.save();
            // Update winner stats
            const winner = await User.findById(winnerId);
            if(!winner){
                return res.status(404).json({ error: 'Winner user not found' });
            }
            winner.stats.totalMatchWon += 1;
            winner.stats.totalGoalsScored += 10;
            winner.stats.streak += 1;
            await winner.save();
            // Update loser stats
            const loser = await User.findById(loserId);
            if(!loser){
                return res.status(404).json({ error: 'Loser user not found' });
            }
            loser.stats.totalMatchLost += 1;
            loser.stats.totalGoalsScored += loserGoals;
            loser.stats.streak = 0;
            await loser.save();
            res.status(201).json(newMatch);
        } catch (error) {
            res.status(500).json({ error: error.message });
        }
    }
}

module.exports = new MatchController();