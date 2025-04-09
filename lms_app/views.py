from django.shortcuts import render

# Create your views here.
def index(request):
    return render(request,'pages/index.html')

def books(request):
    return render(request,'pages/books.html')

def update(request):
    return render(request,'pages/update.html')

def delete(request):
    return render(request,'pages/delete.html')