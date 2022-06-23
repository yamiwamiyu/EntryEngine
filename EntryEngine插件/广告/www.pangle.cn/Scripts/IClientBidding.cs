using System;

namespace ByteDance.Union
{
    public interface IClientBidding
    {
        /// <summary>
        /// invoke this method when bidding successfully
        /// </summary>
        /// <param name="price">price</param>
        void setAuctionPrice(double price);
        
        /// <summary>
        /// invoke after bidding win
        /// </summary>
        /// <param name="price">price</param>
        void win(double price);
        
        /// <summary>
        /// invoke after bidding failed
        /// </summary>
        /// <param name="price">price</param>
        /// <param name="reason">reason</param>
        /// <param name="bidder">bidder</param>
        void Loss(double price, string reason, string bidder);
    }
}