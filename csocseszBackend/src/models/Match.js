const mongoose = require("mongoose");

const matchSchema = new mongoose.Schema(
    {
        id: {
            type: mongoose.Schema.Types.ObjectId,
            unique: true,
            sparse: true
        },
        winnerId: {
            type: mongoose.Schema.Types.ObjectId,
            ref: 'User',
            required: true
        },
        loserId: {
            type: mongoose.Schema.Types.ObjectId,
            ref: 'User',
            required: true
        },
        loserGoals: {
            type: Number,
            required: true,
            min: [0, "MATCH.VALIDATION.LOSER_GOALS_MIN"]
        }
    }, { timestamps: true, versionKey: false });

module.exports = mongoose.model("Match", matchSchema);