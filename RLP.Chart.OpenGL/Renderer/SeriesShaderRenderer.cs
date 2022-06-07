using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTkWPFHost.Core;
using RLP.Chart.OpenGL.Abstraction;

namespace RLP.Chart.OpenGL.Renderer
{
    /// <summary>
    /// 系列渲染器组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SeriesShaderRenderer<T> : BaseRenderer, IEnumerable<T>
        where T : IShaderRendererItem
    {
        protected readonly ConcurrentDictionary<Guid, T> LineRendererDictionary
            = new ConcurrentDictionary<Guid, T>();

        protected readonly IProducerConsumerCollection<T> DeInitializeRendererCollection =
            new ConcurrentBag<T>();

        public IEnumerable<T> EnabledRendererItems =>
            LineRendererDictionary.Values.Where(item => item.RenderEnable);

        public SeriesShaderRenderer(Shader shader)
        {
            Shader = shader;
        }

        public Shader Shader { get; protected set; }

        /// <summary>
        /// 在一个循环中，实际参与渲染的项列表
        /// </summary>
        protected IList<T> RenderWorkingList = new T[] { };

        /// <summary>
        /// 参与渲染的渲染器快照，目标是实现线程安全，保证一次渲染过程在同一个集合上操作
        /// </summary>
        public IReadOnlyList<T> RenderSnapList =>
            new ReadOnlyCollection<T>(RenderWorkingList);

        protected IGraphicsContext Context;

        public override bool AnyReadyRenders()
        {
            return RenderWorkingList.Any();
        }

        public override void Initialize(IGraphicsContext context)
        {
            if (IsInitialized)
            {
                return;
            }

            Context = context;
            this.Shader.Build();
            foreach (var item in LineRendererDictionary.Values)
            {
                item.Initialize(context);
                item.BindShader(Shader);
            }

            this.IsInitialized = true;
        }

        /// <summary>
        /// 检查初始化、反初始化和渲染必要性
        /// 渲染必要性的检查逻辑见返回值解释
        /// </summary>
        /// <returns>true：上次渲染项目和本次渲染项不同，或任一可用渲染项提交了渲染请求(该渲染项的<see cref="PreviewRender"/>也为true；
        /// <para>false：两次渲染的项目相同</para></returns>
        public override bool PreviewRender()
        {
            var renderEnable = false;
            while (DeInitializeRendererCollection.TryTake(out var renderer))
            {
                renderer.Uninitialize();
            }

            var lineRendererCollection = LineRendererDictionary.Values;
            //检查未初始化，当预渲染ok且渲染可用时表示渲染可用
            foreach (var lineRenderer in lineRendererCollection)
            {
                if (!lineRenderer.IsInitialized)
                {
                    lineRenderer.Initialize(Context);
                    lineRenderer.BindShader(Shader);
                }

                if (lineRenderer.PreviewRender() && lineRenderer.RenderEnable)
                {
                    renderEnable = true;
                }
            }

            //对比渲染快照
            var participateItems = lineRendererCollection.Where(renderer => renderer.RenderEnable)
                .ToArray();
            if (!RenderWorkingList.SequenceEqual(participateItems))
            {
                renderEnable = true;
            }

            RenderWorkingList = participateItems;
            return renderEnable;
        }

        public override void Render(GlRenderEventArgs args)
        {
            if (RenderWorkingList.Count == 0)
            {
                return;
            }

            ConfigShader(args);
            foreach (var rendererItem in RenderWorkingList)
            {
                rendererItem.Render(args);
            }
        }

        /// <summary>
        /// 在系列实际渲染前，应用shader参数
        /// </summary>
        /// <param name="args"></param>
        protected virtual void ConfigShader(GlRenderEventArgs args)
        {
            this.Shader.Use();
            this.Shader.SetMatrix4("transform", _directive.Transform);
        }

        private RenderDirective _directive;

        public override void ApplyDirective(RenderDirective directive)
        {
            this._directive = directive;
        }

        public override void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            foreach (var lineRenderer in DeInitializeRendererCollection)
            {
                lineRenderer.Uninitialize();
            }

            var lineRendererCollection = LineRendererDictionary.Values;
            foreach (var lineRenderer in lineRendererCollection)
            {
                lineRenderer.Uninitialize();
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            GL.DeleteProgram(this.Shader.Handle);
            IsInitialized = false;
        }

        public void Add(T renderer)
        {
            this.LineRendererDictionary.TryAdd(renderer.Id, renderer);
        }

        public void AddRange(IEnumerable<T> seriesRendererEnumerable)
        {
            foreach (var lineRenderer in seriesRendererEnumerable)
            {
                this.LineRendererDictionary.TryAdd(lineRenderer.Id, lineRenderer);
            }
        }

        public void Remove(T renderer)
        {
            if (LineRendererDictionary.TryRemove(renderer.Id, out var t) && renderer.IsInitialized)
            {
                DeInitializeRendererCollection.TryAdd(t);
            }
        }

        public void Clear()
        {
            while (!LineRendererDictionary.IsEmpty)
            {
                var lineRendererCollection = LineRendererDictionary.Values;
                foreach (var lineRenderer in lineRendererCollection)
                {
                    this.Remove(lineRenderer);
                }
            }
        }

        public void Restore(IEnumerable<T> seriesRendererEnumerable)
        {
            Clear();
            AddRange(seriesRendererEnumerable);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return LineRendererDictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}