const mongoose = require("mongoose");
let pushUpMultiplier = 3;
class MatchController {
    async getAllMatches(req, res) {
        try {
            let showOnly;
            if (req.body && req.body.project) {
                showOnly = req.body.project;
            }
            let pushUpZ = 0, pushupH = 0, totalCount = 0;
            const Match = mongoose.model('Match');
            let matches = await Match.find().populate('winnerId loserId').lean();
            matches = matches.map(match => {
                totalCount++;
                return match;
            });
            if (showOnly && typeof showOnly === 'string') {
                const fields = showOnly.split(' ').filter(Boolean);
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
            const Match = mongoose.model('Match');
            const User = mongoose.model('User');
            const { winnerId, loserId, loserGoals } = req.body;

            // Create new match record
            const newMatch = new Match({ winnerId, loserId, loserGoals });
            await newMatch.save();
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
            loser.stats.totalMatchLost += 1;
            loser.stats.totalGoalsScored += loserGoals;
            loser.stats.streak = 0;
            loser.stats.totalPushUps += (10 - loserGoals) * pushUpMultiplier;
            await loser.save();
            res.status(201).json(newMatch);
        } catch (error) {
            res.status(500).json({ error: error.message });
        }
    }

    async deleteMatches(req, res) {
        try {
            const Match = mongoose.model('Match');
            await Match.deleteMany({});
            res.status(200).json({ message: 'All matches deleted successfully' });
        } catch (error) {
            res.status(500).json({ error: 'Internal Server Error' });
        }
    }
}

module.exports = new MatchController();