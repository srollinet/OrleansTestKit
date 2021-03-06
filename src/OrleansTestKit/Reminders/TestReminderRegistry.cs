﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Orleans.Runtime;
using Orleans.Timers;

namespace Orleans.TestKit.Reminders
{
    public class TestReminderRegistry : IReminderRegistry
    {
        IRemindable _grain;

        private readonly Dictionary<string, TestReminder> _reminders = new Dictionary<string, TestReminder>();

        public readonly Mock<IReminderRegistry> Mock = new Mock<IReminderRegistry>();

        internal void SetGrainTarget(IRemindable grain)
        {
            _grain = grain;
        }

        public async Task<IGrainReminder> GetReminder(string reminderName) {
            await Mock.Object.GetReminder(reminderName);

            return _reminders[reminderName] as IGrainReminder;
        }
        public async Task<List<IGrainReminder>> GetReminders() {
            await Mock.Object.GetReminders();
            return _reminders.Values.ToList<IGrainReminder>();
        }
        public async Task<IGrainReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            await Mock.Object.RegisterOrUpdateReminder(reminderName, dueTime, period);

            var reminder = new TestReminder(reminderName, dueTime, period);
            _reminders[reminderName] = reminder;

            return reminder;
        }

        public async Task UnregisterReminder(IGrainReminder reminder)
        {
            await Mock.Object.UnregisterReminder(reminder);
            _reminders.Remove(reminder.ReminderName);
        }

        public Task FireReminder(string reminderName, TickStatus tickStatus)
        {
            if (!_reminders.ContainsKey(reminderName))
                throw new ArgumentException($"No reminder named {reminderName} found");

            return _grain.ReceiveReminder(reminderName, tickStatus);
        }

        public async Task FireAllReminders(TickStatus tickStatus)
        {
            foreach(var reminderName in _reminders.Keys)
            {
                await _grain.ReceiveReminder(reminderName, tickStatus);
            }
        }
    }

    public sealed class TestReminder : IGrainReminder
    {
        public string ReminderName { get; }
        public TimeSpan DueTime { get; }
        public TimeSpan Period { get; }

        public TestReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            this.ReminderName = reminderName;
            this.DueTime = dueTime;
            this.Period = period;
        }
    }
}
