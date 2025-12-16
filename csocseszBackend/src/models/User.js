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
    totalPushUps: {
      type: Number,
      default: 0,
    }
  },
  {
    _id: false, 
    toJSON: { virtuals: true },
    toObject: { virtuals: true },
  }
);

// statsSchema.save(function () {
//   const total = this.totalMatchWon + this.totalMatchLost;
//   if (total === 0) return 0;
//   return parseFloat((this.totalMatchWon / total).toFixed(4));
// });

const userSchema = new mongoose.Schema(
  {
    id: {
      type: mongoose.Schema.Types.ObjectId,
      unique: true,
      sparse: true
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
    }
  },
  {
    timestamps: true,
    versionKey: false
  }
);

module.exports = mongoose.model("User", userSchema);