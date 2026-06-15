using System;
using System.Collections;
using System.Collections.Generic;
using Controller.Utils;
using Data.Utils;
using MEC;
using UIs.Utils;
using UnityEngine;

//This class handles events
namespace Utils
{
    public class EventsManager : IDisposable
    {
        private readonly Config config;
        private readonly MonoBehaviour owner;
        private bool updating;

        public EventsManager(MonoBehaviour owner, Config config)
        {
            this.owner = owner;
            this.config = config;

            //attach listeners
            // if (config.added != null)
            //     foreach (var metaId in config.added)
            //     {
            //         BGRepo.I.GetField(metaId).ValueChanged -= Handler;
            //         BGRepo.I.GetField(metaId).ValueChanged += Handler;
            //     }
            if (config.updatedField != null)
                foreach (var field in config.updatedField)
                {
                    field.ValueChanged -= Handler;
                    field.ValueChanged += Handler;
                }
                
            if (config.updatedEntity != null)
                foreach (var entity in config.updatedEntity)
                {
                    entity.AnyValueChanged -= Handler;
                    entity.AnyValueChanged += Handler;
                }
                
            if (config.updatedUI != null)
                foreach (var entity in config.updatedUI)
                {
                    entity.ValueChanged -= Handler;
                    entity.ValueChanged += Handler;
                }
                
            if (config.updatedController != null)
                foreach (var entity in config.updatedController)
                {
                    entity.ValueChanged -= Handler;
                    entity.ValueChanged += Handler;
                }
        }

        public void Reconnect(bool doHandler = true)
        {
            if (config.updatedField != null)
                foreach (var field in config.updatedField)
                {
                    field.ValueChanged -= Handler;
                    field.ValueChanged += Handler;
                    if (doHandler) Handler();
                }
            
            if (config.updatedEntity != null)
                foreach (var entity in config.updatedEntity)
                {
                    entity.AnyValueChanged -= Handler;
                    entity.AnyValueChanged += Handler;
                    if (doHandler) Handler();
                }
                
            if (config.updatedUI != null)
                foreach (var entity in config.updatedUI)
                {
                    entity.ValueChanged -= Handler;
                    entity.ValueChanged += Handler;
                    if (doHandler) Handler();
                }
                
            if (config.updatedController != null)
                foreach (var entity in config.updatedController)
                {
                    entity.ValueChanged -= Handler;
                    entity.ValueChanged += Handler;
                    if (doHandler) Handler();
                }
        }

        public void Dispose()
        {
            if (config.updatedField != null)
                foreach (var field in config.updatedField)
                    field.ValueChanged -= Handler;
            
            if (config.updatedEntity != null)
                foreach (var entity in config.updatedEntity)
                    entity.AnyValueChanged -= Handler;
            
            if (config.updatedUI != null)
                foreach (var entity in config.updatedUI)
                    entity.ValueChanged -= Handler;
            
            if (config.updatedController != null)
                foreach (var entity in config.updatedController)
                    entity.ValueChanged -= Handler;
            //remove listeners
            // if (config.added != null)
            //     foreach (var metaId in config.added)
            //         BGRepo.I.Events.RemoveAnyEntityAddedListener(metaId, Handler);
            // if (config.deleted != null)
            //     foreach (var metaId in config.deleted)
            //         BGRepo.I.Events.RemoveAnyEntityDeletedListener(metaId, Handler);
        }

        private void Handler()
        {
            if (updating || owner == null) return;

            updating = true;
            Timing.RunCoroutine(_DoUpdate());
        }
        
        private void Handler(object sender, DbEventArgs e)
        {
            if (updating || owner == null) return;

            updating = true;
            //we do not call config.handler() directly here, cause we want to call it only once per frame
            Timing.RunCoroutine(_DoUpdate());
        }

        private IEnumerator<float> _DoUpdate()
        {
            yield return Timing.WaitForOneFrame;
            config.handler();
            updating = false;
        }

        public void Set(Action newHandler, DbField[] newUpdated)
        {
            Dispose();
            config.handler = newHandler;
            config.updatedField = newUpdated;
            Reconnect();
        }


        public void Set(Action newHandler, DbUserModel[] newUpdated)
        {
            Dispose();
            config.handler = newHandler;
            config.updatedEntity = newUpdated;
            Reconnect();
        }


        public void Set(Action newHandler, ControllerField[] newUpdated)
        {
            Dispose();
            config.handler = newHandler;
            config.updatedController = newUpdated;
            Reconnect();
        }
        
        public class Config
        {
            public Action handler;
            public DbField[] updatedField;
            public DbUserModel[] updatedEntity;
            public UIField[] updatedUI;
            public ControllerField[] updatedController;
        }
    }
}