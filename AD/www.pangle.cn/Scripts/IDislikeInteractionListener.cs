//------------------------------------------------------------------------------
// Copyright (c) 2018-2019 Beijing Bytedance Technology Co., Ltd.
// All Right Reserved.
// Unauthorized copying of this file, via any medium is strictly prohibited.
// Proprietary and confidential.
//------------------------------------------------------------------------------

namespace ByteDance.Union
{
    /// <summary>
    /// The interaction listener for dislike.
    /// </summary>
    public interface IDislikeInteractionListener
    {
        /// <summary>
        /// Invoke when the dislike is selected.
        /// @param position 选择的位置
        /// @param value 选择的内容
        /// @param enforce 是否强制关闭广告
        /// </summary>
        void OnSelected(int var1, string var2, bool enforce);

        /// <summary>
        /// Invoke when the dislike is cancel.
        /// </summary>
        void OnCancel();

        /// <summary>
        /// Refuse to submit again.
        /// </summary>
        void OnShow();
    }
}
