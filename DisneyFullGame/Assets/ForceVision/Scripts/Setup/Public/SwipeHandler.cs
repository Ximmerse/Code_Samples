using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Disney.ForceVision
{
	public abstract class SwipeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public enum SwipeDirection
		{
			Left,
			Right
		}

		public RectTransform Screens;
		public CanvasScaler Scaler;

		private const float SwipeTolerance = 0.5f;
		private const float MinimumSwipeDistance = 100f;

		private float screenWidth = 0f;

		protected float ScreenWidth
		{
			get
			{
				if (screenWidth == 0)
				{
					screenWidth = Screens.rect.width * (Scaler != null ? Scaler.scaleFactor : 1);
				}

				return screenWidth;
			}
		}

		protected int CurrentStepInt = 0;
		private Vector2 firstInput;
		private Vector2 secondInput;

		protected virtual void DetectSwipe()
		{
			Vector2 currentInput = Vector2.zero;

			// getting delta between first and second input
			currentInput = new Vector2(secondInput.x - firstInput.x, secondInput.y - firstInput.y);

			// checking that swipe met the min distance requirement
			if (Mathf.Abs(currentInput.x) < MinimumSwipeDistance && Mathf.Abs(currentInput.y) < MinimumSwipeDistance)
			{
				return;
			}

			// normalize the input vector
			currentInput.Normalize();

			// left swipe
			if (currentInput.x < 0 && currentInput.y > -SwipeTolerance && currentInput.y < SwipeTolerance)
			{
				OnSwipeHandler(SwipeDirection.Left);
			}
			// right swipe
			if (currentInput.x > 0 && currentInput.y > -SwipeTolerance && currentInput.y < SwipeTolerance)
			{
				OnSwipeHandler(SwipeDirection.Right);
			}
		}

		private void OnSwipeHandler(SwipeDirection direction)
		{
			int nextStep = CurrentStepInt - 1;

			if (direction == SwipeDirection.Left)
			{
				nextStep = CurrentStepInt + 1;
			}

			UpdateScreen(nextStep);
		}

		protected void AnimateToScreen(bool shouldAnimate)
		{
			Screens.DOLocalMoveX(CurrentStepInt * -ScreenWidth, shouldAnimate ? 0.5f : 0f).SetEase(Ease.OutQuad);
		}

		protected abstract void UpdateScreen(int step);

		#region IBeginDragHandler

		public void OnBeginDrag(PointerEventData eventData)
		{
			firstInput = eventData.position;
		}

		#endregion

		public void OnDrag(PointerEventData eventData)
		{
		}

		#region IDragHandler

		#endregion

		#region IEndDragHandler

		public void OnEndDrag(PointerEventData eventData)
		{
			secondInput = eventData.position;

			DetectSwipe();
		}

		#endregion
	}
}


