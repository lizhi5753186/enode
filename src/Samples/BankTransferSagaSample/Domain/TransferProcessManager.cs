﻿using System;
using BankTransferSagaSample.Events;
using ENode.Domain;
using ENode.Eventing;

namespace BankTransferSagaSample.Domain
{
    [Serializable]
    public class TransferProcessManager : AggregateRoot<Guid>,
        IEventHandler<TransferProcessStarted>,
        IEventHandler<TransferOutRequested>,
        IEventHandler<TransferedOutHandled>,
        IEventHandler<TransferInRequested>,
        IEventHandler<TransferedInHandled>
    {
        public TransferState CurrentTransferState { get; private set; }
        public ProcessState CurrentProcessState { get; private set; }

        public TransferProcessManager() : base() { }
        public TransferProcessManager(BankAccount sourceAccount, BankAccount targetAccount, double amount) : base(Guid.NewGuid())
        {
            RaiseEvent(
                new TransferProcessStarted(
                    Id, sourceAccount.Id,
                    targetAccount.Id,
                    amount,
                    string.Format("转账流程启动，源账户：{0}，目标账户：{1}，转账金额：{2}", sourceAccount.AccountNumber, targetAccount.AccountNumber, amount)));
            RaiseEvent(new TransferOutRequested(Id, sourceAccount.Id, targetAccount.Id, amount));
        }

        public void HandleTransferedOut(TransferedOut evnt)
        {
            RaiseEvent(new TransferedOutHandled(Id, evnt.SourceAccountId, evnt.TargetAccountId, evnt.Amount));
            RaiseEvent(new TransferInRequested(Id, evnt.SourceAccountId, evnt.TargetAccountId, evnt.Amount));
        }
        public void HandleTransferedIn(TransferedIn evnt)
        {
            RaiseEvent(new TransferedInHandled(Id, evnt.SourceAccountId, evnt.TargetAccountId, evnt.Amount));
        }

        void IEventHandler<TransferProcessStarted>.Handle(TransferProcessStarted evnt)
        {
            CurrentProcessState = ProcessState.Started;
        }
        void IEventHandler<TransferOutRequested>.Handle(TransferOutRequested evnt)
        {
            CurrentProcessState = ProcessState.TransferOutRequested;
        }
        void IEventHandler<TransferedOutHandled>.Handle(TransferedOutHandled evnt)
        {
            CurrentTransferState = TransferState.TransferedOut;
        }
        void IEventHandler<TransferInRequested>.Handle(TransferInRequested evnt)
        {
            CurrentProcessState = ProcessState.TransferInRequested;
        }
        void IEventHandler<TransferedInHandled>.Handle(TransferedInHandled evnt)
        {
            CurrentTransferState = TransferState.TransferedIn;
            CurrentProcessState = ProcessState.Completed;
        }

        public enum ProcessState
        {
            NotStarted,
            Started,
            TransferOutRequested,
            TransferInRequested,
            Completed
        }
        public enum TransferState
        {
            None,
            TransferedOut,
            TransferedIn
        }
    }
}
