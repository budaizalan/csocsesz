const mongoose = require("mongoose");

const goalSchema = new mongoose.Schema(
    {
        side: {
            type: String,
            enum: ['blue', 'red'],
            required: true
        },
        time: {
            type: Date,
            required: true,
            default: Date.now
        }
    }, { _id: false });

const matchSchema = new mongoose.Schema(
    {
        id: {
            type: mongoose.Schema.Types.ObjectId,
            unique: true,
            sparse: true
        },
        startTime: {
            type: Date,
            required: true,
        },
        matchDuration: {
            type: Number,
            default: function() {
                return Date.now() - this.startTime;
            }
        },
        goals: [{
            type: goalSchema,
            default: []
        }],
        winnerId: {
            type: mongoose.Schema.Types.ObjectId,
            ref: 'User',
            required: true
        },
        winnerSide: {
            type: String,
            enum: ['blue', 'red'],
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
            default: function() {
                if (!this.winnerSide || !Array.isArray(this.goals)) return 0;
                return this.goals.filter(g => g.side !== this.winnerSide).length;
            },
            min: [0, "MATCH.VALIDATION.LOSER_GOALS_MIN"]
        },
        pushUpsMultiplier: {
            type: Number,
            required: true,
            min: [0, "MATCH.VALIDATION.PUSHUPS_MIN"]
        }
    }, { timestamps: true, versionKey: false }, {
        toJSON: {
            transform: (doc, ret) => {
                ret.id = doc._id;
                delete ret._id;
                delete ret.__v;
                return ret;
            }
        }
    });

module.exports = mongoose.model("Match", matchSchema);