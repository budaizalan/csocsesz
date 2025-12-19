const mongoose = require("mongoose");
let pushUpMultiplier = 3;
// const Match = mongoose.model('Match');
const Match = require('../models/Match');

class MatchController {
    async getAllMatches(req, res) {
        try {
            let project;
            if (req.body && req.body.project) {
                project = req.body.project;
            }
            let pushUpZ = 0, pushupH = 0, totalCount = 0;
            let matches = await Match.find().populate('winnerId loserId').lean();
            matches = matches.map(match => {
                totalCount++;
                return match;
            });
            if (project && typeof project === 'string') {
                const fields = project.split(' ').filter(Boolean);
                matches = matches.map(match => {
                    const projected = {};
                    fields.forEach(field => {
                        const parts = field.split('.');
                        let value = match;

                        for (const part of parts) {
                            if (value && value.hasOwnProperty(part)) {
                                value = value[part];
                            } else {
                                value = undefined;
                                break;
                            }
                        }

                        if (value !== undefined) {
                            let target = projected;
                            for (let i = 0; i < parts.length - 1; i++) {
                                if (!target[parts[i]]) {
                                    target[parts[i]] = {};
                                }
                                target = target[parts[i]];
                            }
                            target[parts[parts.length - 1]] = value;
                        }
                    });
                    return projected;
                });
            }

            res.status(200).json({ pushUpZ, pushupH, totalCount, matches });
        }
        catch (error) {
            res.status(500).json({ error: error.message });
        }
    }

    async postMatch(req, res) {
        try {
            console.log(JSON.stringify(req.body, null, 2));
            const User = mongoose.model('User');
            const { winnerId, loserId, startTime, matchDuration, winnerSide, pushUpsMultiplier, goals } = req.body;
            let filteredGoals = goals.filter(goal => goal !== null && goal !== undefined);
            // Create new match record
            const newMatch = new Match({ winnerId, loserId, startTime, matchDuration, winnerSide, pushUpsMultiplier, goals: filteredGoals });
            const savedMatch = await newMatch.save();
             // Update winner stats
             const winner = await User.findById(winnerId);
             if (!winner) {
                 return res.status(404).json({ error: 'Winner user not found' });
             }
             winner.stats.totalMatchWon += 1;
             winner.stats.totalGoalsScored += 10;
             winner.stats.streak += 1;
             await winner.save();
             // Update loser stats
             const loser = await User.findById(loserId);
             if (!loser) {
                 return res.status(404).json({ error: 'Loser user not found' });
             }
            const computedLoserGoals = (typeof savedMatch.loserGoals === 'number') ? savedMatch.loserGoals : 0;
            const multiplier = (typeof savedMatch.pushUpsMultiplier === 'number') ? savedMatch.pushUpsMultiplier : pushUpMultiplier;

            loser.stats.totalMatchLost += 1;
            loser.stats.totalGoalsScored += computedLoserGoals;
            loser.stats.streak = 0;
            loser.stats.totalPushUps += (10 - computedLoserGoals) * multiplier;
             await loser.save();
            res.status(201).json(savedMatch);
         } catch (error) {
             res.status(500).json({ error: error.message });
         }
     }

    async deleteMatches(req, res) {
        try {
            await Match.deleteMany({});
            res.status(200).json({ message: 'All matches deleted successfully' });
        } catch (error) {
            res.status(500).json({ error: 'Internal Server Error' });
        }
    }

    async deleteMatchById(req, res){
        try {
            const id = req.params.id;
            let doc = Match.findByIdAndDelete(id);
            res.status(200).json({ "Delete doc": doc});
        } catch (error) {
            res.status(500).json({ error: 'Internal Server Error'});
        }
    }
}

module.exports = new MatchController();