using Infrastructure.InputControl;
using UnityEngine;
using VContainer;

namespace Features.Character.CharacterSrc
{
    public class CharacterMoveController
    {
        private readonly CharacterView view;
        private readonly InputService input;
        private float moveSpeed;
        private bool isMoving;


        [Preserve]
        public CharacterMoveController(CharacterView view, InputService input)
        {
            this.view = view;
            this.input = input;
        }


        public void SetMoveSpeed(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }


        public void Tick()
        {
            float dt = Time.deltaTime;
            
            if (dt <= 0)
                return;

            view.Tick(input.X, input.Y, in dt);
        }


        public void FixedTick()
        {
            float dt = Time.fixedDeltaTime;
            
            if (dt <= 0)
                return;

            if (!input.HasInput())
            {
                StopMove();
                return;
            }

            view.FixedTick(input.X, input.Y, in moveSpeed, in dt);
            isMoving = true;
        }


        private void StopMove()
        {
            if (!isMoving) 
                return;
            
            view.StopMove();
            isMoving = false;
        }
    }
}