const mongoose = require("mongoose");

const statsSchema = new mongoose.Schema(
  {
    streak: {
      type: Number,
      default: 0,
      min: [0, "USER.VALIDATION.STATS_NEGATIVE_STREAK"],
    },
    totalMatchWon: {
      type: Number,
      default: 0,
      min: [0, "USER.VALIDATION.STATS_NEGATIVE_MATCH_WON"],
    },
    totalMatchLost: {
      type: Number,
      default: 0,
      min: [0, "USER.VALIDATION.STATS_NEGATIVE_MATCH_LOST"],
    },
  },
  {
    _id: false, 
    toJSON: { virtuals: true },
    toObject: { virtuals: true },
  }
);

statsSchema.virtual("winRate").get(function () {
  const total = this.totalMatchWon + this.totalMatchLost;
  if (total === 0) return 0;
  return parseFloat((this.totalMatchWon / total).toFixed(4));
});

const userSchema = new mongoose.Schema(
  {
    id: {
      type: mongoose.Schema.Types.ObjectId,
      unique: true,
    },
    name: {
      type: String,
      required: [true, "USER.VALIDATION.NAME_REQUIRED"],
      trim: true,
      minlength: [3, "USER.VALIDATION.NAME_MINLENGTH"],
      maxlength: [30, "USER.VALIDATION.NAME_MAXLENGTH"],
    },
    stats: {
      type: statsSchema,
      default: () => ({}),
    },

    inGame: {
      type: new mongoose.Schema(
        {
          matchWon: { type: Number, default: 0 },
          goals: { type: Number, default: 0 },
          side: {
            type: String,
            enum: {
              values: ["red", "blue"],
              message: "USER.VALIDATION.INVALID_SIDE",
            },
            required: [true, "USER.VALIDATION.SIDE_REQUIRED"],
          },
        },
        { _id: false }
      ),
      default: null,
    },

    lastActiveAt: {
      type: Date,
      default: Date.now,
    },
  },
  {
    timestamps: true,
  }
);

module.exports = mongoose.model("User", userSchema);