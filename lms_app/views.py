from django.shortcuts import render

# Create your views here.
def demo(request):
    return render(request,'lms_app/index.html')